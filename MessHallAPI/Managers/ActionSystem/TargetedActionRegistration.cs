using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MelonLoader;
using MessHallAPI.Debugger;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class TargetedActionRegistration
    {
        private static readonly Dictionary<ProximityTargetedAction,
            (UIInteractButton Button, Action<int> OnUse, Action OnUpdate, string ActionName, string Keybind, ITargetedActionHandler Handler)>
            _targetedActions = new();

        private const string IconParentPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/" +
            "LowerRightParent/UI_TargetedActionIcon";

        private const string TrackerButtonPath = IconParentPath + "/SM_UI_Tracker_Button_01";

        private const int VanillaActionMax = 7;
        private static int _nextActionId = VanillaActionMax + 1;

        public static ProximityTargetedAction AllocateAction() => (ProximityTargetedAction)_nextActionId++;

        public static void Register(ProximityTargetedAction action, ITargetedActionHandler handler)
        {
            if (!_targetedActions.TryAdd(action, (null, handler.OnUseTarget, handler.OnUpdate, handler.ActionName, handler.Keybind, handler)))
            {
                Logging.Warn($"TargetedActionRegistration: Action {action} already registered, skipping.");
                return;
            }
            Logging.Log($"TargetedActionRegistration: Registered action {action}.");
        }

        public static void Unregister(ProximityTargetedAction action) => _targetedActions.Remove(action);

        public static void AutoRegister()
        {
            foreach (MelonMod mod in MelonTypeBase<MelonMod>.RegisteredMelons)
            {
                foreach (Type type in mod.GetType().Assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<TargetedActionDefinitionAttribiute>() == null) continue;

                    if (!typeof(ITargetedActionHandler).IsAssignableFrom(type))
                    {
                        Logging.Warn($"TargetedActionRegistration: {type.Name} has [TargetedActionDefinition] but no valid handler interface.");
                        continue;
                    }

                    var handler = (ITargetedActionHandler)Activator.CreateInstance(type);
                    Logging.DebugLog($"TargetedActionRegistration: AutoRegister found handler {type.Name}.");
                    Register(AllocateAction(), handler);
                }
            }
        }

        public static void Dispatch(ProximityTargetedAction action, int callerPlayerID, int targetID)
        {
            Logging.DebugLog($"TargetedActionRegistration: Dispatch action={action} caller={callerPlayerID} target={targetID}.");
            PlayerState target = null;
            foreach (PlayerState ps in Spawn.ActivePlayerStates)
                if (ps.PlayerId == targetID) { target = ps; break; }

            if (_targetedActions.TryGetValue(action, out var entry))
            {
                Logging.DebugLog($"TargetedActionRegistration: Invoking OnUse for action {action}.");
                entry.OnUse.Invoke(target?.PlayerId ?? targetID);
            }
            else
            {
                Logging.Warn($"TargetedActionRegistration: No handler registered for action {action}.");
            }
        }

        public static void OnUpdate()
        {
            try
            {
                if (!InGame || Client.PState == null) return;

                foreach (var (action, entry) in _targetedActions)
                {
                    var iconObj = GameObject.Find($"{IconParentPath}/SM_TargetedAction_{action}_Button");
                    if (iconObj == null)
                    {
                        Logging.DebugLog($"TargetedActionRegistration: Icon not found for action {action}.");
                        continue;
                    }

                    bool enabled = entry.Handler.IsEnabled();
                    Logging.DebugLog($"TargetedActionRegistration: Action {action} IsEnabled={enabled}.");
                    iconObj.SetActive(enabled);
                    if (!enabled) continue;

                    var glyph = iconObj.transform.Find("3D_BindingGlyph/GlyphKBM");
                    if (glyph == null)
                        Logging.DebugLog($"TargetedActionRegistration: No GlyphKBM found for action {action}.");
                    glyph?.GetComponent<LUITile>()?.SetTileOffset(StringToV2(entry.Keybind));
                }

                foreach (var (action, entry) in _targetedActions)
                {
                    if (!entry.Handler.IsEnabled()) continue;

                    bool correctState = entry.Handler.isMeetingAction
                        ? GameState.InVotingState() && !GameState.InLobbyState()
                        : GameState.InTaskState();

                    Logging.DebugLog($"TargetedActionRegistration: Action {action} isMeetingAction={entry.Handler.isMeetingAction} correctState={correctState}.");

                    if (!correctState) continue;

                    TargetOutlineManager.OutlineTarget(action, action, 0, entry.Button);

                    if (!Keyboard.current.allKeys[StringToKeybind(entry.Keybind.ToLower())].wasPressedThisFrame) continue;

                    Logging.DebugLog($"TargetedActionRegistration: Keybind pressed for action {action}. Targets: {Killing._targetPlayers.Count}.");

                    if (Killing._targetPlayers.Count == 0) continue;

                    Logging.DebugLog($"TargetedActionRegistration: Firing RPC_UseAction for action {action}.");
                    TargetedActionHandler.RPC_UseAction((int)action, Client.PState.PlayerId, Killing._targetPlayers[0].PlayerId);
                }
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }

        public static void BuildIcons()
        {
            Logging.DebugLog($"TargetedActionRegistration: BuildIcons called for {_targetedActions.Count} action(s).");
            var trackerObj = GameObject.Find(TrackerButtonPath);
            if (trackerObj == null)
            {
                Logging.Error("TargetedActionRegistration: Could not find tracker button to clone.");
                return;
            }

            foreach (var (action, entry) in _targetedActions)
                BuildIcon(action, entry, trackerObj);
        }

        public static void BuildIcon(ProximityTargetedAction action, (UIInteractButton Button, Action<int> OnUse, Action OnUpdate, string ActionName, string Keybind, ITargetedActionHandler Handler) entry, GameObject trackerObj)
        {
            Logging.DebugLog($"TargetedActionRegistration: BuildIcon for action {action} (name: {entry.ActionName}).");

            var cloned = GameObject.Instantiate(trackerObj, trackerObj.transform.parent);
            cloned.name = $"SM_TargetedAction_{action}_Button";

            var button = cloned.GetComponent<UIInteractButton>();
            if (button == null)
            {
                Logging.Error($"TargetedActionRegistration: Cloned object for {action} has no UIInteractButton.");
                return;
            }

            if (!IsKeyAccepted(entry.Keybind))
            {
                Logging.Error($"'{entry.Keybind}' is not a valid key for action {action}, falling back to {eKey}.");
                entry = entry with { Keybind = eKey };
            }

            var label = cloned.transform.Find("NameLabel")?.GetComponent<TextMeshPro>();
            if (label != null)
            {
                label.text = entry.ActionName;
                Logging.DebugLog($"TargetedActionRegistration: Set label to '{entry.ActionName}' for action {action}.");
            }
            else
            {
                Logging.Error($"TargetedActionRegistration: No NameLabel found for action {action}.");
            }

            _targetedActions[action] = entry with { Button = button };
            Logging.DebugLog($"TargetedActionRegistration: BuildIcon complete for action {action}.");
        }
    }
}