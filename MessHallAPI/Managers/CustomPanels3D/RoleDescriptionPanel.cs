using Il2CppTMPro;
using MessHallAPI.Debugger;
using UnityEngine;
using static Custom3DPanelManager;

namespace MessHallAPI.Managers.CustomPanels3D
{
    [PanelDefinition]
    public class RoleDescriptionPanel : CustomPanel
    {
        public override string PanelName => "RoleInfo";
        public override string Keybind => "none";
        public override Custom3DPanelManager.PanelOpenTrigger OpenTrigger => Custom3DPanelManager.PanelOpenTrigger.Manual;

        private static string _selectedName;
        private static string _selectedDesc;
        private static string _selectedTeam;
        private static Sprite _selectedIcon;
        private static Color _selectedColor;
        private static GameObject _cachedPanel;

        public static void SetSelectedRole(string name, string desc, string team, Sprite icon, Color teamColor)
        {
            _selectedName = name;
            _selectedDesc = desc;
            _selectedTeam = team;
            _selectedIcon = icon;
            _selectedColor = teamColor;

            Logging.Log($"[RoleDescriptionPanel] Role set: {name} | {desc} | {team} | {teamColor}");

            if (_cachedPanel != null)
                Refresh(_cachedPanel);
        }

        public override void OnPanelCreated(GameObject panel)
        {
            _cachedPanel = panel;

            var sourceTf = panel.transform.Find("PlayersPanel/UI_Moderation/UI_Moderation_PlayerPanel/Name Text");
            if (sourceTf == null)
            {
                Logging.Error("[RoleDescriptionPanel] NameText template not found.");
                return;
            }

            CreateTMP(sourceTf, panel, "RoleName", 0.5f, new Vector3(0.3592f, 0.1842f, -0.1313f));
            CreateTMP(sourceTf, panel, "RoleTeam", 0.25f, new Vector3(0.3592f, 0.1380f, -0.1313f));
            CreateTMP(sourceTf, panel, "RoleDescription", 0.25f, new Vector3(0.3592f, 0.0531f, -0.1313f));

            var iconGo = UnityEngine.Object.Instantiate(sourceTf, panel.transform);
            iconGo.name = "RoleSprite";
            UnityEngine.Object.Destroy(iconGo.GetComponent<TextMeshPro>());
            UnityEngine.Object.DestroyImmediate(iconGo.GetComponent<MeshRenderer>());
            iconGo.gameObject.AddComponent<SpriteRenderer>().sprite = ModStorage.ModStamp;
            iconGo.transform.localPosition = new Vector3(0.444f, 0.1765f, -0.1699f);
            iconGo.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

            Logging.Log("[RoleDescriptionPanel] OnPanelCreated complete.");
        }

        public override void OnPanelOpened(GameObject panel)
        {
            panel.transform.Find("PlayersPanel")?.gameObject.SetActive(false);

            if (_selectedName != null)
                Refresh(panel);
        }
        
        private static void Refresh(GameObject panel)
        {
            if (_selectedName == null) return;

            SetTMP(panel, "RoleName", _selectedName);
            SetTMP(panel, "RoleDescription", _selectedDesc);

            var teamTf = panel.transform.Find("RoleTeam");
            if (teamTf != null)
            {
                var tmp = teamTf.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = _selectedTeam;
                    tmp.color = _selectedColor;
                }
            }

            var spriteTf = panel.transform.Find("RoleSprite");
            if (spriteTf != null)
            {
                var sr = spriteTf.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = _selectedIcon;
            }

            Logging.Log($"[RoleDescriptionPanel] Refreshed: {_selectedName} | {_selectedDesc} | {_selectedTeam}");
        }

        private static void CreateTMP(Transform source, GameObject parent, string objName, float fontSize, Vector3 localPos)
        {
            var go = UnityEngine.Object.Instantiate(source, parent.transform);
            go.name = objName;
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one;
            go.GetComponent<TextMeshPro>().fontSize = fontSize;
        }

        private static void SetTMP(GameObject panel, string objName, string value)
        {
            var tmp = panel.transform.Find(objName)?.GetComponent<TextMeshPro>();
            if (tmp != null) tmp.text = value;
        }
    }
}