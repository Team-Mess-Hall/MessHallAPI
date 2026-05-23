using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MessHallAPI.Debugger;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class TargetedActionRegistration
    {
        private static readonly Dictionary<ProximityTargetedAction, (UIInteractButton? Button, Action<int> OnUse, Action? OnUpdate, string ActionName, string Keybind, ITargetedActionHandler Handler)> _targetedActions = new();
        private static PlayerState Caller = null!;
        private static PlayerState Target = null!;

        private const string IconParentPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerRightParent/UI_TargetedActionIcon";

        private const string TrackerButtonPath =
            IconParentPath + "/SM_UI_Tracker_Button_01";

        private const int VanillaActionMax = 7;
        private static int _nextActionId = VanillaActionMax + 1;

        public static ProximityTargetedAction AllocateAction() => (ProximityTargetedAction)_nextActionId++;

        public static void Register(ProximityTargetedAction action, ITargetedActionHandler handler)
        {
            if (!_targetedActions.TryAdd(action, (null, handler.OnUseTarget, handler.OnUpdate, handler.ActionName, handler.Keybind, handler)))
            {
                Logging.Warn($"TargetedActionRegistration: Action {action} is already registered, skipping.");
                return;
            }

            Logging.Log($"TargetedActionRegistration: Registered action {action}.");
        }

        public static void Unregister(ProximityTargetedAction action)
        {
            _targetedActions.Remove(action);
        }

        public static void Dispatch(ProximityTargetedAction action, int CallerPlayerID, int TargetID)
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId == CallerPlayerID)
                    Caller = player;

                if (player.PlayerId == TargetID)
                    Target = player;
            }

            if (_targetedActions.TryGetValue(action, out var entry))
                entry.OnUse.Invoke(Target.PlayerId);
            else
                Logging.Warn($"TargetedActionRegistration: No handler registered for action {action}.");
        }

        public static void OnUpdate()
        {
            try
            {
                if (!InGame || Client.PState == null)
                    return;

                foreach (var (action, entry) in _targetedActions)
                {
                    var iconObj = GameObject.Find($"{IconParentPath}/SM_TargetedAction_{action}_Button");
                    if (iconObj == null)
                        continue;

                    bool isEnabled = entry.Handler.IsEnabled();
                    iconObj.SetActive(isEnabled);

                    if (!isEnabled)
                        continue;

                    var bindingGlyph = iconObj.transform.Find("3D_BindingGlyph");
                    if (bindingGlyph != null)
                    {
                        var glyphKBM = bindingGlyph.Find("GlyphKBM");
                        if (glyphKBM != null)
                        {
                            var tile = glyphKBM.GetComponent<LUITile>();
                            if (tile != null)
                                tile.SetTileOffset(StringToV2(entry.Keybind));
                        }
                    }
                }

                foreach (var (action, entry) in _targetedActions)
                {
                    if (!entry.Handler.IsEnabled())
                        continue;

                    bool inCorrectState = entry.Handler.isMeetingAction
                        ? GameState.InVotingState() && !GameState.InLobbyState()
                        : GameState.InTaskState();

                    if (!inCorrectState)
                        continue;

                    TargetOutlineManager.OutlineTarget(action, action, 0, entry.Button);

                    if (!Keyboard.current.allKeys[StringToKeybind(entry.Keybind.ToLower())].wasPressedThisFrame)
                        continue;

                    if (Killing._targetPlayers.Count == 0)
                        continue;

                    TargetedActionHandler.RPC_UseAction(
                        (int)action,
                        Client.PState.PlayerId,
                        Killing._targetPlayers[0].PlayerId
                    );
                }
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }

        public static void BuildIcon(ProximityTargetedAction action, (UIInteractButton? Button, Action<int> OnUse, Action? OnUpdate, string ActionName, string Keybind, ITargetedActionHandler Handler) entry, GameObject trackerObj)
        {
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
                Logging.Error($"{entry.Keybind} is not a valid key, falling back to default.");
                entry = entry with { Keybind = eKey };
            }

            var label = cloned.transform.Find("NameLabel");
            if (label != null)
            {
                var text = label.GetComponent<TextMeshPro>();
                if (text != null)
                    text.text = entry.ActionName;
            }

            _targetedActions[action] = entry with { Button = button };
            Logging.DebugLog($"TargetedActionRegistration: Built icon for action {action}.");
        }

        public static void BuildIcons()
        {
            var trackerObj = GameObject.Find(TrackerButtonPath);
            if (trackerObj == null)
            {
                Logging.Error("TargetedActionRegistration: Could not find tracker button to clone.");
                return;
            }

            foreach (var (action, entry) in _targetedActions)
                BuildIcon(action, entry, trackerObj);
        }

        public static void AutoRegister()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = type.GetCustomAttribute<TargetedActionDefinitionAttribiute>();
                if (attr == null) continue;

                if (typeof(ITargetedActionHandler).IsAssignableFrom(type))
                {
                    var handler = (ITargetedActionHandler)Activator.CreateInstance(type);
                    Register(AllocateAction(), handler);
                    continue;
                }

                Logging.Warn($"TargetedActionRegistration: {type.Name} has [TargetedActionDefinition] but no valid handler interface.");
            }
        }
    }
}