using TMPro;
using SG.Airlock.Roles;
using MessHallAPI.Debugger;
using MessHallAPI.Managers;
using MessHallAPI.Managers.CustomPanels3D;
using MessHallAPI.Managers.Role;
using UnityEngine;
using static Custom3DPanelManager;
using static MessHallAPI.Managers.KeybindManager;
using SG.LightUI;

[PanelDefinition]
public class RoleSelectionPanel : CustomPanel
{
    public override string PanelName => "RoleSelection";
    public override string Keybind => fKey;
    public override PanelOpenTrigger OpenTrigger => PanelOpenTrigger.Keybind;

    private const int Columns = 3;
    private const int Rows = 2;
    private const int PageSize = Columns * Rows;
    private int _currentPage = 0;
    private List<RoleEntry> _roles;
    private GameObject _sourceTMP;
    private const string pathToButton = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/UpperRightParent/PlayerList Icon";
    public static LUITile kbm;

    private record RoleEntry(string RoleName, string RoleDesc, string RoleTeam, Color TeamColor, Sprite RoleIcon, bool IsVanilla);

    private static Color VanillaTeamColor(string team) => team switch
    {
        "Impostor" => Color.red,
        "Neutral" => Color.gray,
        "Crewmate" => Color.cyan,
        _ => Color.white
    };

    private static Color CustomTeamColor(RoleData data) => data.Team switch
    {
        GameTeam.Crewmember => data._crewmemberTeamColor,
        GameTeam.Impostor => data._impostorTeamColor,
        GameTeam.Infected => data._infectedTeamColor,
        _ => data._otherTeamColor
    };

    private List<RoleEntry> BuildRoleList()
    {
        var list = new List<RoleEntry>();

        foreach (var r in VanillaRoleManager.GetRegisteredRoles())
            list.Add(new RoleEntry(r.RoleName, r.RoleDesc, r.RoleTeam, VanillaTeamColor(r.RoleTeam), r.RoleIcon, true));

        foreach (var r in CustomRoleManager.GetRegisteredRoles())
        {
            RoleData data;
            try { data = r.BuildRoleData(); }
            catch (Exception ex)
            {
                Logging.Error($"[RoleSelectionPanel] BuildRoleData failed for '{r.RoleName}': {ex}");
                continue;
            }
            list.Add(new RoleEntry(r.RoleName, r.RoleDesc, data.Team.ToString(), CustomTeamColor(data), r.RoleIcon, false));
        }

        return list;
    }

    public override void OnPanelOpened(GameObject panel)
    {
        panel.transform.Find("PlayersPanel")?.gameObject.SetActive(false);

        _roles = BuildRoleList();
        _currentPage = 0;

        if (_sourceTMP == null)
        {
            Logging.Error("[RoleSelectionPanel] OnPanelOpened: SourceTMP not cached.");
            return;
        }

        RefreshPage(panel);
    }

    public override void OnPanelCreated(GameObject panel)
    {
        var nameTextTransform = panel.transform.Find("PlayersPanel/UI_Moderation/UI_Moderation_PlayerPanel/Name Text");
        if (nameTextTransform == null)
        {
            Logging.Error("[RoleSelectionPanel] OnPanelCreated: Could not find Name Text.");
            return;
        }
        _sourceTMP = nameTextTransform.gameObject;

        var sourceIcon = GameObject.Find(pathToButton);
        if (sourceIcon != null)
        {
            var cloned = GameObject.Instantiate(sourceIcon, sourceIcon.transform.parent);
            cloned.name = "RoleSelection_HUDIcon";
            cloned.transform.localPosition = new Vector3(-0.3688f, -0.06f, 0f);

            var person0 = cloned.transform.Find("Person 0");
            if (person0 != null)
                person0.GetComponent<UnityEngine.UI.Image>().sprite = ModStorage.RoleButton;

            kbm = cloned.transform.Find("3D_BindingGlyph/GlyphKBM").GetComponent<LUITile>();
        }
        else
        {
            Logging.Error("[RoleSelectionPanel] OnPanelCreated: Could not find PlayerList Icon to clone.");
        }

        var prevBtn = new GameObject("PrevPage");
        prevBtn.transform.SetParent(panel.transform, false);
        prevBtn.layer = LayerMask.NameToLayer("UI");
        prevBtn.transform.localPosition = new Vector3(-0.261f, 0.0423f, -0.1316f);
        prevBtn.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        prevBtn.AddComponent<SpriteRenderer>().sprite = ModStorage.LeftArrow;
        new CustomButtonSystem
        {
            Target = prevBtn,
            OnPressed = () =>
            {
                int totalPages = (int)Math.Ceiling(_roles.Count / (float)PageSize);
                _currentPage = (_currentPage - 1 + totalPages) % totalPages;
                RefreshPage(panel);
            }
        };

        var nextBtn = new GameObject("NextPage");
        nextBtn.transform.SetParent(panel.transform, false);
        nextBtn.layer = LayerMask.NameToLayer("UI");
        nextBtn.transform.localPosition = new Vector3(0.5882f, 0.0423f, -0.1568f);
        nextBtn.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        nextBtn.AddComponent<SpriteRenderer>().sprite = ModStorage.RightArrow;
        new CustomButtonSystem
        {
            Target = nextBtn,
            OnPressed = () =>
            {
                int totalPages = (int)Math.Ceiling(_roles.Count / (float)PageSize);
                _currentPage = (_currentPage + 1) % totalPages;
                RefreshPage(panel);
            }
        };

        Logging.Log("[RoleSelectionPanel] OnPanelCreated complete.");
    }

    private void BuildPage(GameObject panel, int page)
    {
        for (int i = 0; i < PageSize; i++)
        {
            panel.transform.Find("RoleBtn_Slot_" + i)?.gameObject.SetActive(false);
            panel.transform.Find("RoleBtn_Slot_" + i + "_Label")?.gameObject.SetActive(false);
        }

        if (_roles == null || _roles.Count == 0) return;

        var pageRoles = _roles.Skip(page * PageSize).Take(PageSize).ToList();

        for (int i = 0; i < pageRoles.Count; i++)
        {
            var entry = pageRoles[i];
            var capturedEntry = entry;

            int col = i % Columns;
            int row = i / Columns;
            float x = -0.0208f + col * 0.2f;
            float y = 0.1767f - row * 0.2f;

            var existingBtn = panel.transform.Find("RoleBtn_Slot_" + i);
            if (existingBtn != null)
            {
                existingBtn.gameObject.SetActive(true);
                existingBtn.GetComponent<SpriteRenderer>().sprite = entry.RoleIcon;

                var existingSys = CustomButtonSystem.GetForTarget(existingBtn.gameObject);
                if (existingSys != null)
                    existingSys.OnPressed = () =>
                    {
                        RoleDescriptionPanel.SetSelectedRole(capturedEntry.RoleName, capturedEntry.RoleDesc, capturedEntry.RoleTeam, capturedEntry.RoleIcon, capturedEntry.TeamColor);
                        Custom3DPanelManager.ClosePanel(panel.name);
                        Custom3DPanelManager.OpenPanel("RoleInfo");
                    };

                var existingLabel = panel.transform.Find("RoleBtn_Slot_" + i + "_Label");
                if (existingLabel != null)
                {
                    existingLabel.gameObject.SetActive(true);
                    existingLabel.GetComponent<TextMeshPro>().text = entry.RoleName;
                }
                continue;
            }

            var btnObj = new GameObject("RoleBtn_Slot_" + i);
            btnObj.transform.SetParent(panel.transform, false);
            btnObj.layer = LayerMask.NameToLayer("UI");
            btnObj.transform.localPosition = new Vector3(x, y, -0.1371f);
            btnObj.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            btnObj.AddComponent<SpriteRenderer>().sprite = entry.RoleIcon;
            new CustomButtonSystem
            {
                Target = btnObj,
                OnPressed = () =>
                {
                    RoleDescriptionPanel.SetSelectedRole(capturedEntry.RoleName, capturedEntry.RoleDesc, capturedEntry.RoleTeam, capturedEntry.RoleIcon, capturedEntry.TeamColor);
                    Custom3DPanelManager.ClosePanel(panel.name);
                    Custom3DPanelManager.OpenPanel("RoleInfo");
                }
            };

            var labelObj = GameObject.Instantiate(_sourceTMP, panel.transform);
            labelObj.name = "RoleBtn_Slot_" + i + "_Label";
            labelObj.layer = LayerMask.NameToLayer("UI");
            labelObj.transform.localPosition = new Vector3(x, y - 0.08f, -0.1371f);
            labelObj.transform.localScale = Vector3.one;
            var label = labelObj.GetComponent<TextMeshPro>();
            label.text = entry.RoleName;
            label.fontSize = 0.15f;
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    private void RefreshPage(GameObject panel) => BuildPage(panel, _currentPage);
}