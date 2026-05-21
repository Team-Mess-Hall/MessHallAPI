using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.Settings;
using Il2CppSG.Airlock.UI;
using Il2CppSG.GlobalEvents.Variables;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Role;
using System.Collections.Generic;
using UnityEngine;

namespace MessHallAPI.Managers.RoleSettings
{
    public static class SettingsManager
    {
        private const int SettingsPerPage = 5;

        private const string RolesSettingsPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerCenterParent/UI_LobbyScreen_3D/LobbyScreenParent/Match Customization/UI_MatchCustomization/Roles Settings";

        private const string PageOnePath = RolesSettingsPath + "/Page One";

        public static string pathToPageOne = PageOnePath;
        public static string pathToEngineer = $"{PageOnePath}/UI_MatchOption_Engineer";
        public static string pathToVigi = $"{PageOnePath}/UI_MatchOption_Vigilante";
        public static string pathToTracker = $"{PageOnePath}/UI_MatchOption_Tracker";
        public static string pathToNumofVIPs = $"{PageOnePath}/UI_MatchOption_NumOfVIPs";
        public static string pathToGA = $"{PageOnePath}/UI_MatchOption_GuardianAngel";

        private static readonly List<GameObject> _spawnedPages = new();
        private static readonly Dictionary<GameObject, (ICustomRole Role, GameRole GameRole)> _slotRoleMap = new();

        public static void BuildSettingsPages()
        {
            var pageOneObj = GameObject.Find(PageOnePath);
            if (pageOneObj == null)
            {
                Logging.Error("[SettingsManager] Could not find PageOne to clone.");
                return;
            }

            var parent = pageOneObj.transform.parent;

            var singleActiveGroup = parent.GetComponent<SingleActiveGroup>();
            if (singleActiveGroup == null)
            {
                Logging.Error("[SettingsManager] Could not find SingleActiveGroup on Roles Settings.");
                return;
            }

            foreach (var page in _spawnedPages)
                GameObject.Destroy(page);
            _spawnedPages.Clear();
            _slotRoleMap.Clear();

            var registeredRoles = CustomRoleManager._roles.ToList();
            int totalRoles = registeredRoles.Count;
            int totalPages = Mathf.CeilToInt(totalRoles / (float)SettingsPerPage);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var newPage = GameObject.Instantiate(pageOneObj, parent);
                newPage.name = $"CustomRolePage_{pageIndex + 1}";
                _spawnedPages.Add(newPage);
                singleActiveGroup._activeGroup.Add(newPage);
                Logging.Log($"[SettingsManager] ActiveGroup count after add: {singleActiveGroup._activeGroup.Count}");

                var slots = new List<Transform>();
                for (int i = 0; i < newPage.transform.childCount; i++)
                    slots.Add(newPage.transform.GetChild(i));

                for (int slotIndex = 0; slotIndex < SettingsPerPage; slotIndex++)
                {
                    int roleIndex = pageIndex * SettingsPerPage + slotIndex;

                    if (slotIndex >= slots.Count)
                        break;

                    var slot = slots[slotIndex];

                    if (roleIndex >= totalRoles)
                    {
                        slot.gameObject.SetActive(false);
                        continue;
                    }

                    var (gameRole, entry) = registeredRoles[roleIndex];
                    ApplySettingsItem(slot, entry.Source, gameRole);
                    _slotRoleMap[slot.gameObject] = (entry.Source, gameRole);
                    slot.gameObject.SetActive(true);

                    Logging.Log($"[SettingsManager] Patched slot for '{entry.Source.RoleName}' — page {pageIndex + 1}, slot {slotIndex + 1}.");
                }
            }

            Logging.Log($"[SettingsManager] Built {totalPages} custom page(s) for {totalRoles} registered role(s).");
        }

        private static void ApplySettingsItem(Transform slot, ICustomRole role, GameRole gameRole)
        {
            var selector = slot.GetComponent<UISelector>();
            if (selector == null)
            {
                Logging.Error($"[SettingsManager] No UISelector on slot for '{role.RoleName}'.");
                slot.gameObject.SetActive(false);
                return;
            }

            var roleData = CustomRoleManager._roles[gameRole].Definition;

            foreach (var selectorValue in selector._selectorValues)
            {
                for (int i = 0; i < selectorValue.SelectorVariablesInt.Length; i++)
                {
                    selectorValue.SelectorVariablesInt[i] = new UISelector.SelectorVariableInt
                    {
                        Variable = new IntSettingsItem
                        {
                            _variable = roleData.MaxNumOfRole,
                        },
                        SetVariableTo = selectorValue.SelectorVariablesInt[i].SetVariableTo
                    };
                }
            }

            var optionText = slot.Find("Option Text").GetComponent<UserStringComponent_TMP>();
            if (optionText == null)
            {
                Logging.Error($"[SettingsManager] No UserStringComponent_TMP on slot for '{role.RoleName}'.");
                return;
            }

            optionText.AssignTextKey(roleData.RoleNameTK);

            var item = new IntSettingsItem
            {
                _variable = roleData.MaxNumOfRole,
            };

            selector.AdjustedBoolSettings.Clear();
            selector.AdjustedFloatSettings.Clear();
            selector.AdjustedIntSettings.Clear();
            selector.AdjustedIntSettings.Add(item);
        }

        public static void OnUpdate()
        {
            if (_spawnedPages.Count == 0)
                return;

            var rolesSettingsObj = GameObject.Find(RolesSettingsPath);
            if (rolesSettingsObj == null)
                return;

            var singleActiveGroup = rolesSettingsObj.GetComponent<SingleActiveGroup>();
            if (singleActiveGroup == null)
                return;

            foreach (var page in _spawnedPages)
            {
                if (!singleActiveGroup._activeGroup.Contains(page))
                {
                    singleActiveGroup._activeGroup.Add(page);
                    Logging.Log($"[SettingsManager] ActiveGroup count after add: {singleActiveGroup._activeGroup.Count}");
                }

                for (int i = 0; i < page.transform.childCount; i++)
                {
                    var slot = page.transform.GetChild(i).gameObject;
                    if (!_slotRoleMap.TryGetValue(slot, out var entry))
                        continue;

                    var roleData = CustomRoleManager._roles[entry.GameRole].Definition;
                    var roleDescComp = slot.GetComponent<UIRoleDescription>();
                    if (roleDescComp == null)
                        continue;

                    roleDescComp._roleDescription = CustomRoleManager._roles[entry.GameRole].text;
                    roleDescComp._roleName = roleData.RoleNameTK;
                    roleDescComp._gameTeam = roleData.Team;
                    roleDescComp._crewOrImpText.text = "<color=#636363>Neutral</color>";
                }
            }
        }
    }
}