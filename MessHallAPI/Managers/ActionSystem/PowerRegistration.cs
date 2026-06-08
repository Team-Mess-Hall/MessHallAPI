using BepInEx.Unity.IL2CPP;
using MessHallAPI.Debugger;
using SG.Airlock;
using SG.Airlock.Roles;
using SG.Airlock.UI;
using SG.LightUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class PowerRegistration
    {
        private static readonly Dictionary<PowerUps,
            (PowerUp Definition, UIInteractButton Button, Action<int> OnUse, Action OnUpdate,
             string PowerName, Func<Sprite> PowerIcon, bool IsTargeted, string Keybind)>
            _powers = new();

        private const string IconParentPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/" +
            "LowerRightParent/UI_PowerUpIcon";

        private const string DisinfectPath = IconParentPath + "/SM_PowerUp_Disinfect_Button";

        private const int AtlasCellSize = 256;
        private const int AtlasWidth = 2048;
        private const int AtlasHeight = 2048;

        private static int _nextPowerId = 256;

        public static PowerUps AllocatePower()
        {
            var allocated = (PowerUps)_nextPowerId;
            _nextPowerId <<= 1;
            return allocated;
        }

        // ── Registration ──────────────────────────────────────────────────────
        public static void Register(PowerUps power, PowerUp definition, Action onUse,
            string powerName, Func<Sprite> powerIcon, string keybind = "E", Action onUpdate = null)
        {
            if (!_powers.TryAdd(power, (definition, null, _ => onUse(), onUpdate, powerName, powerIcon, false, keybind)))
            {
                Logging.Warn($"PowerRegistration: Power {power} already registered, skipping.");
                return;
            }
            Logging.Log($"PowerRegistration: Registered power {power}.");
        }

        public static void RegisterTargeted(PowerUps power, PowerUp definition, Action<int> onUse,
            string powerName, Func<Sprite> powerIcon, string keybind = "E", Action onUpdate = null)
        {
            if (!_powers.TryAdd(power, (definition, null, onUse, onUpdate, powerName, powerIcon, true, keybind)))
            {
                Logging.Warn($"PowerRegistration: Targeted power {power} already registered.");
                return;
            }
            Logging.Log($"PowerRegistration: Registered targeted power {power}.");
        }

        public static void Unregister(PowerUps power) => _powers.Remove(power);

        public static void AutoRegister()
        {
            foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
            {
                if (pluginInfo.Instance == null) continue;

                foreach (Type type in pluginInfo.Instance.GetType().Assembly.GetTypes())
                {
                    bool hasPowerAttr = type.GetCustomAttribute<PowerUpDefinition>() != null;
                    if (!hasPowerAttr) continue;

                    if (typeof(IPowerUpHandler).IsAssignableFrom(type))
                    {
                        var handler = (IPowerUpHandler)Activator.CreateInstance(type);
                        var allocated = AllocatePower();
                        if (handler is CustomPower cp) cp.AllocatedType = allocated;
                        CustomPowerRegistration.Track(type, allocated);
                        Register(allocated, handler.Definition, handler.OnUse, handler.PowerName,
                            () => handler.PowerIcon, handler.Keybind, handler.OnUpdate);
                        continue;
                    }

                    if (typeof(ITargetedPowerHandler).IsAssignableFrom(type))
                    {
                        var handler = (ITargetedPowerHandler)Activator.CreateInstance(type);
                        var allocated = AllocatePower();
                        if (handler is CustomTargetedPower ctp) ctp.AllocatedType = allocated;
                        CustomPowerRegistration.Track(type, allocated);
                        RegisterTargeted(allocated, handler.Definition, handler.OnUseTarget,
                            handler.PowerName, () => handler.PowerIcon, handler.Keybind, handler.OnUpdate);
                        continue;
                    }

                    Logging.Warn($"PowerRegistration: {type.Name} has [PowerUpDefinition] but no valid handler interface.");
                }
            }
        }

        public static void Dispatch(PowerUps power, int callerPlayerID)
        {
            if (_powers.TryGetValue(power, out var entry))
                entry.OnUse.Invoke(callerPlayerID);
            else
                Logging.Warn($"PowerRegistration: No handler registered for power {power}.");
        }

        public static void DispatchTarget(PowerUps power, int callerPlayerID, int targetID)
        {
            if (_powers.TryGetValue(power, out var entry))
            {
                entry.OnUse.Invoke(targetID);

                foreach (PlayerState ps in Spawn.ActivePlayerStates)
                    if (ps.PlayerId == callerPlayerID) { ps.ActivePowerUps = PowerUps.None; break; }
            }
            else
            {
                Logging.Warn($"PowerRegistration: No targeted handler registered for power {power}.");
            }
        }

        public static List<PowerUp> CreateAll()
        {
            var list = new List<PowerUp>();
            foreach (var (power, entry) in _powers)
            {
                list.Add(entry.Definition);
                Logging.DebugLog($"PowerRegistration: Created power {power}.");
            }
            return list;
        }

        public static void OnUpdate()
        {
            try
            {
                if (!InGame || Client.PState == null) return;

                var active = Client.PState.ActivePowerUps;
                if (!CustomPowerHandler.IsCustomPower((int)active)) return;
                if (!_powers.TryGetValue(active, out var entry)) return;

                var iconObj = GameObject.Find($"{IconParentPath}/SM_PowerUp_{active}_Button");
                if (iconObj != null)
                {
                    var glyph = iconObj.transform.Find("3D_BindingGlyph/GlyphKBM");
                    glyph?.GetComponent<LUITile>()?.SetTileOffset(StringToV2(entry.Keybind));
                }

                if (entry.Definition.TargetedAction != ProximityTargetedAction.None)
                    TargetOutlineManager.OutlineTarget(
                        entry.Definition.TargetedAction,
                        entry.Definition.TargetedAction,
                        entry.Definition.Duration,
                        entry.Button);

                if (!Keyboard.current.allKeys[StringToKeybind(entry.Keybind.ToLower())].wasPressedThisFrame) return;

                if (entry.IsTargeted)
                {
                    if (Killing._targetPlayers.Count == 0) return;
                    CustomPowerHandler.RPC_UsePowerTarget(
                        (int)active, Client.PState.PlayerId, Killing._targetPlayers[0].PlayerId);
                }
                else
                {
                    CustomPowerHandler.RPC_UsePower((int)active, Client.PState.PlayerId);
                }
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }

        public static void BuildIcons()
        {
            var parentObj = GameObject.Find(IconParentPath);
            if (parentObj == null) { Logging.Error("PowerRegistration: Could not find icon parent."); return; }

            var iconParent = parentObj.GetComponent<PowerUpIconParent>();
            if (iconParent == null) { Logging.Error("PowerRegistration: Icon parent has no PowerUpIconParent component."); return; }

            var disinfectObj = GameObject.Find(DisinfectPath);
            if (disinfectObj == null) { Logging.Error("PowerRegistration: Could not find Disinfect button to clone."); return; }

            foreach (var (power, entry) in _powers)
                BuildIcon(power, entry, disinfectObj, iconParent);
        }

        public static void BuildIcon(
            PowerUps power,
            (PowerUp Definition, UIInteractButton Button, Action<int> OnUse, Action OnUpdate,
             string PowerName, Func<Sprite> PowerIcon, bool IsTargeted, string Keybind) entry,
            GameObject disinfectObj,
            PowerUpIconParent iconParent)
        {
            var cloned = GameObject.Instantiate(disinfectObj, disinfectObj.transform.parent);
            cloned.name = $"SM_PowerUp_{power}_Button";

            var button = cloned.GetComponent<UIInteractButton>();
            if (button == null) { Logging.Error($"PowerRegistration: Cloned object for {power} has no UIInteractButton."); return; }

            var mr = cloned.GetComponent<MeshRenderer>();
            if (mr == null) { Logging.Error($"PowerRegistration: Cloned object for {power} has no MeshRenderer."); return; }

            if (!IsKeyAccepted(entry.Keybind))
            {
                Logging.Error($"'{entry.Keybind}' is not a valid key for power {power}, falling back to {eKey}.");
                entry = entry with { Keybind = eKey };
            }

            var label = cloned.transform.Find("NameLabel")?.GetComponent<TextMeshPro>();
            if (label != null) label.text = entry.PowerName;

            iconParent._icons.Add(new PowerUpIconParent.Icon { Type = power, Button = button });
            _powers[power] = entry with { Button = button };

            Logging.DebugLog($"PowerRegistration: Built icon for power {power}.");
        }
    }
}