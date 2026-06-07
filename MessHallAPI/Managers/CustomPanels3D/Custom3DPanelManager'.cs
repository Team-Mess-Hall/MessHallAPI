using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.UI.Moderation;
using Il2CppSG.Airlock.XR;
using Il2CppTMPro;
using MelonLoader;
using MessHallAPI.Debugger;
using MessHallAPI.Managers;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Base.References;
using static MessHallAPI.Managers.KeybindManager;

public static class Custom3DPanelManager
{
    private const string PlayerListPath = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/UI_Minimap3D/OffsetRotation/BGOffset/Center/PlayerList";
    private const string CenterPath = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/UI_Minimap3D/OffsetRotation/BGOffset/Center";

    public static readonly Dictionary<string, GameObject> extraPanels = new();
    private static readonly List<ICustomPanel> _pendingPanels = new();
    private static readonly Dictionary<string, ICustomPanel> _registeredPanels = new();

    public static void Register(ICustomPanel panel)
    {
        if (_pendingPanels.Any(p => p.PanelName == panel.PanelName))
        {
            Logging.Warn($"Custom3DPanelManager: Panel {panel.PanelName} already queued.");
            return;
        }

        _pendingPanels.Add(panel);
        Logging.Log($"Custom3DPanelManager: Queued panel {panel.PanelName}");
    }

    public static void FlushPanels()
    {
        var playerlist = GameObject.Find(PlayerListPath);
        if (playerlist == null) return;

        var allPanels = _pendingPanels.Concat(
            _registeredPanels.Values.Where(p => !extraPanels.TryGetValue(p.PanelName, out var go) || go == null)
        ).ToList();

        foreach (var panel in allPanels)
        {
            if (extraPanels.TryGetValue(panel.PanelName, out var existing) && existing == null)
                extraPanels.Remove(panel.PanelName);

            if (extraPanels.ContainsKey(panel.PanelName)) continue;

            var clone = GameObject.Instantiate(playerlist, playerlist.transform.parent);
            clone.name = panel.PanelName;
            clone.SetActive(false);

            var playersPanel = clone.transform.Find("PlayersPanel");

            var reportPanel = playersPanel?.Find("UI_ReportPlayer");
            if (reportPanel != null)
                GameObject.Destroy(reportPanel.gameObject);

            var reportAbuseInstructions = playersPanel?.Find("UI_ReportAbuseInstructions");
            if (reportAbuseInstructions != null)
                GameObject.Destroy(reportAbuseInstructions.gameObject);

            var moderationPanel = playersPanel?.Find("UI_Moderation");
            if (moderationPanel != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    var playerPanelName = i == 0 ? "UI_Moderation_PlayerPanel" : $"UI_Moderation_PlayerPanel ({i})";
                    var playerPanel = moderationPanel.Find(playerPanelName);
                    if (playerPanel == null) continue;

                    var tab = playerPanel.GetComponent<ModerationPlayerTab>();
                    if (tab != null)
                        tab._isLocalPlayer = true;

                    var backButtonCollider = playerPanel.Find("BackButtonCollider");
                    if (backButtonCollider == null) continue;

                    int playerId = i == 0 ? 9 : i - 1;

                    var minigameButton = backButtonCollider.GetComponent<MinigameButton>();
                    if (minigameButton != null)
                        minigameButton.OnButtonPressed.AddListener(new Action<XRHand>((hand) =>
                        {
                            panel.OnPlayerSelected(clone, playerId);
                            Logging.Log($"Custom3DPanelManager: Selected slot {playerId}.");
                        }));
                }
            }

            if (panel.OpenTrigger == PanelOpenTrigger.Keybind && !IsKeyAccepted(panel.Keybind))
                Logging.Error($"Custom3DPanelManager: {panel.Keybind} is not a valid key for panel {panel.PanelName}, falling back to {eKey}.");

            var nameTextTransform = playerlist.transform.Find("PlayersPanel/UI_Moderation/UI_Moderation_PlayerPanel/Name Text");
            if (nameTextTransform != null)
            {
                var closeBtn = GameObject.Instantiate(nameTextTransform, clone.transform);
                closeBtn.name = "CloseButton";
                GameObject.DestroyImmediate(closeBtn.GetComponent<TextMeshPro>());
                GameObject.DestroyImmediate(closeBtn.GetComponent<MeshRenderer>());
                closeBtn.gameObject.AddComponent<SpriteRenderer>().sprite = ModStorage.CloseButton;
                closeBtn.transform.localPosition = new Vector3(-0.261f, 0.2814f, -0.1316f);
                closeBtn.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

                var panelName = panel.PanelName;
                var closeBtnSys = new CustomButtonSystem
                {
                    Target = closeBtn.gameObject,
                    OnPressed = () => ClosePanel(panelName)
                };
            }

            panel.OnPanelCreated(clone);
            extraPanels[panel.PanelName] = clone;
            _registeredPanels.TryAdd(panel.PanelName, panel);
        }

        _pendingPanels.Clear();
    }

    public static void OnUpdate()
    {
        var center = GameObject.Find(CenterPath);
        if (center != null && !center.activeSelf)
        {
            foreach (var (name, panelObj) in extraPanels)
            {
                if (panelObj != null && panelObj.activeSelf)
                    ClosePanel(name);
            }
        }

        foreach (var (name, panel) in _registeredPanels)
        {
            if (panel.OpenTrigger != PanelOpenTrigger.Keybind) continue;
            if (!IsKeyAccepted(panel.Keybind)) continue;

            if (Keyboard.current.allKeys[StringToKeybind(panel.Keybind.ToLower())].wasPressedThisFrame)
                TogglePanel(name);
        }
    }

    public static void OpenPanel(string panelName)
    {
        if (!extraPanels.TryGetValue(panelName, out var panel)) return;

        var center = GameObject.Find(CenterPath);
        if (center != null)
            center.SetActive(true);

        panel.SetActive(true);

        if (_registeredPanels.TryGetValue(panelName, out var customPanel))
            customPanel.OnPanelOpened(panel);
    }

    public static void ClosePanel(string panelName)
    {
        if (!extraPanels.TryGetValue(panelName, out var panel)) return;


        var center = GameObject.Find(CenterPath);
        if (center != null)
            center.SetActive(false);

        panel.SetActive(false);

        if (_registeredPanels.TryGetValue(panelName, out var customPanel))
            customPanel.OnPanelClosed(panel);
    }

    public static void TogglePanel(string panelName)
    {
        if (!extraPanels.TryGetValue(panelName, out var panel)) return;
        if (panel.activeSelf)
            ClosePanel(panelName);
        else
            OpenPanel(panelName);
    }

    public static void AutoRegisterPanels()
    {
        foreach (var mod in MelonMod.RegisteredMelons)
        {
            var assembly = mod.GetType().Assembly;
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<PanelDefinitionAttribute>();
                if (attr == null) continue;

                if (!typeof(ICustomPanel).IsAssignableFrom(type))
                {
                    Logging.Warn($"{type.Name} has [PanelDefinition] but does not implement ICustomPanel.");
                    continue;
                }

                var panel = (ICustomPanel)Activator.CreateInstance(type)!;
                Register(panel);
            }
        }
    }

    public enum PanelOpenTrigger
    {
        /// <summary>
        /// Keybind field is requiredand when said keybind is press panel opens.
        /// </summary>
        Keybind,
        /// <summary>
        /// Allows you to activate the panel manually, doesnt require a keybind.
        /// </summary>
        Manual
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PanelDefinitionAttribute : Attribute
    {
        public PanelDefinitionAttribute() { }
    }

    public interface ICustomPanel
    {
        string PanelName { get; }
        PanelOpenTrigger OpenTrigger { get; }
        string Keybind { get; }
        /// <summary>
        /// this method is called when the panel finishes creating and can have anything hooked into it, EX: An AnimationClip for Vitals
        /// </summary>
        /// <param name="panel">the custom panel that was made</param>
        void OnPanelCreated(GameObject panel);
        /// <summary>
        /// THis method allows you to do specific stuff like call an rpc on the player you select, EX: an rpc that makes the shapeshift as another player
        /// </summary>
        /// <param name="panel">The panel that was used</param>
        /// <param name="playerID">The playerID that was selected</param>
        void OnPlayerSelected(GameObject panel, int playerID);
        /// <summary>
        /// THis method is called when the panel is opened use this for specific actions when the panel is opened
        /// </summary>
        /// <param name="panel">the panel that was opened</param>
        void OnPanelOpened(GameObject panel);
        /// <summary>
        /// THis method is called when the panel is closed use this for specific actions when the panel is closed
        /// </summary>
        /// <param name="panel">the panel that was opened</param>
        void OnPanelClosed(GameObject panel);
    }

    public abstract class CustomPanel : ICustomPanel
    {
        public abstract string PanelName { get; }
        public abstract PanelOpenTrigger OpenTrigger { get; }
        public virtual string Keybind => eKey;
        public virtual void OnPanelCreated(GameObject panel) { }
        public virtual void OnPlayerSelected(GameObject panel, int player) { }
        public virtual void OnPanelOpened(GameObject panel) { }
        public virtual void OnPanelClosed(GameObject panel) { }
    }
}