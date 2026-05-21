using Harmony;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI;
using Il2CppSG.GlobalEvents.Variables;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static MelonLoader.MelonLogger;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.Role
{
    /*
        NOTES:

        Role = required needs to be a custom value for custom role

        RoleNameTK = TextKey required
        RolePromptTK = TextKey required
        EjectionReveal = TextKey required

        ChanceOfRoleAssignment = FloatVariable optional

        VentIcon = required if CanFreelyVent is true

        CanFreelyVent = required
        CanCallSabotages = required
        CanDoTasks = required
        CanUsePowerups = required

        IsAssignedAtStart = required
        IsAssignedTasks = required
        IsUniversalKnowledge = required

        GestureHandColor = not changable

        MaxNumOfRole = IntVariable optional

        IsGhostRole = required

        PlayerSpeedMultiplier = optional defaults to 1

        TargetedActionRadius = not changable

        VisionRange = optional defaults to 1

        Team = required

        CameraHeight = required to be 1.334f

        _targetedAction = optional

        Team colors do not need to be filled
        but should be set if corresponding team is used

        _ghostTargetedAction = optional

        _ghostRoleAssignedSFX = optional

        _ghostTargetedActionCooldown = optional
        mainly used for ghost roles with buttons

        _maxTimeInVents = optional

        _selfActions = optional

        _shortenedCooldown = not changable

        _targetedActionCooldown = optional
        mainly used if role has an action

        _ventUseCooldown = optional
    */

    public static class CustomRoleManager
    {
        public static readonly Dictionary<GameRole, (RoleData Definition, string RoleName, string RoleDescription, string RoleRevealPrompt, ICustomRole Source, TextKey text)> _roles = new();
        private static readonly List<(GameRole Role, ICustomRole Source)> _pendingRoles = new();
        private static readonly Dictionary<int, int> _roleCache = new();
        private const int VanillaRoleMax = 10;
        private static int _nextRoleId = VanillaRoleMax + 1;

        public static GameRole AllocateRole() => (GameRole)_nextRoleId++;

        public static void Register(GameRole role, ICustomRole customRole)
        {
            if (_roles.ContainsKey(role))
            {
                Logging.Warn($"CustomRoleManager: Role {role} already registered.");
                return;
            }

            _pendingRoles.Add((role, customRole));
            Logging.Log($"CustomRoleManager: Queued role {customRole.RoleName}");
        }

        public static void FlushRoles()
        {
            try
            {
                if (roleManager == null || roleManager._availableRoles == null)
                    return;

                foreach (var (role, source) in _pendingRoles)
                {
                    var roleData = source.BuildRoleData();
                    roleData.Role = role;

                    roleData.RoleNameTK = CreateTextKey();
                    roleData.RolePromptTK = CreateTextKey();
                    roleData.EjectionReveal = CreateTextKey();
                    roleData.ChanceOfRoleAssignment = CreateRoleFloatVar();
                    roleData.MaxNumOfRole = CreateRoleIntVar();
                    if (roleData._targetedAction != ProximityTargetedAction.None)
                    {
                        roleData._targetedActionCooldown = CreateRoleCooldownIntVar();
                    }
                    RegisterTextKey(source.RoleName, roleData.RoleNameTK);
                    RegisterTextKey(source.RoleRevealPrompt, roleData.RolePromptTK);
                    RegisterTextKey($"was a <color=#{ColorToHex(roleData.RoleNameColor)}>{source.RoleName}</color>.", roleData.EjectionReveal);

                    if (!roleManager._availableRoles.AllRoles.Contains(roleData))
                        roleManager._availableRoles.AllRoles.Add(roleData);
                    AddRoleDataToList(roleData);

                    if (roleManager.gameRoleToPlayerIds != null && !roleManager.gameRoleToPlayerIds.ContainsKey(role))
                    {
                        roleManager.gameRoleToPlayerIds[role] = new Il2CppSystem.Collections.Generic.List<int>();
                        Logging.Log($"CustomRoleManager: Initialized gameRoleToPlayerIds entry for {source.RoleName}");
                    }
                    var settingsDescKey = CreateTextKey();
                    RegisterTextKey(source.RoleDesc, settingsDescKey);
                    _roles[role] = (roleData, source.RoleName, source.RoleDesc, source.RoleRevealPrompt, source, settingsDescKey);
                }

                _pendingRoles.Clear();
            }
            catch (Exception e)
            {
                Logging.Log(e.ToString());
            }
        }

        public static void AddRoleDataToList(RoleData role)
        {
            roleManager._availableRoles._filteredAvailableRoles.Add(role);
        }
        public static void Unregister(GameRole role)
        {
            if (!_roles.TryGetValue(role, out var entry))
                return;

            if (roleManager != null && roleManager._availableRoles != null)
            {
                roleManager._availableRoles.AllRoles.Remove(entry.Definition);
            }

            _roles.Remove(role);

            Logging.Log($"CustomRoleManager: Unregistered role {role}");
        }

        public static bool IsCustomRole(GameRole role)
        {
            return _roles.ContainsKey(role);
        }

        public static TextKey CreateTextKey()
        {
            var template = roleManager._availableRoles.AllRoles[3];

            return UnityEngine.Object.Instantiate(template.EjectionReveal);
        }

        public static FloatVariable CreateRoleFloatVar()
        {
            var template = roleManager._availableRoles.AllRoles[3];

            return UnityEngine.Object.Instantiate(template.ChanceOfRoleAssignment);
        }

        public static IntVariable CreateRoleIntVar()
        {
            var template = roleManager._availableRoles.AllRoles[3];

            return UnityEngine.Object.Instantiate(template.MaxNumOfRole);
        }

        public static IntVariable CreateRoleCooldownIntVar()
        {
            var template = roleManager._availableRoles.AllRoles[1];

            return UnityEngine.Object.Instantiate(template._targetedActionCooldown);
        }

        public static void RegisterTextKey(string text, TextKey textKey)
        {
            var template = roleManager._availableRoles.AllRoles[0].EjectionReveal._localizationManager._currentTextDatabase;
            template.AddNewEntry(textKey, text);
        }

        public static void AutoRegisterRoles()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = type.GetCustomAttribute<RoleDefinitionAttribute>();
                if (attr == null) continue;

                if (!typeof(ICustomRole).IsAssignableFrom(type))
                {
                    Logging.Warn($"{type.Name} has [RoleDefinition] but does not implement ICustomRole.");
                    continue;
                }

                var role = (ICustomRole)Activator.CreateInstance(type)!;
                Register(AllocateRole(), role);
            }
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void RPC_SendRoleToAll(int playerID, int gameRole)
        {
            var role = (GameRole)gameRole;
            _roleCache[playerID] = (int)role;
            Logging.Log($"[RoleSync] Received role {role} for player {playerID}");
        }
        public static void GetTrueRole(int PlayerID)
        {
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<GameRole, Il2CppSystem.Collections.Generic.List<int>> roleEntry in roleManager.gameRoleToPlayerIds)
            {
                bool found = false;
                foreach (int id in roleEntry.Value)
                {
                    if (id == PlayerID)
                    {
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SendRoleToAll", PlayerID, (int)roleEntry.Key);
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }

        public static GameRole GetTrueRoleHost(int PlayerID)
        {
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<GameRole, Il2CppSystem.Collections.Generic.List<int>> roleEntry in roleManager.gameRoleToPlayerIds)
            {
                foreach (int id in roleEntry.Value)
                {
                    if (id == PlayerID)
                    {
                        return roleEntry.Key;
                    }
                }
            }
            return GameRole.NotSet;
        }

        public static bool TryGetCachedRole(int playerID, out int role)
        {
            return _roleCache.TryGetValue(playerID, out role);
        }

        public static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }
        public static IReadOnlyList<ICustomRole> GetRegisteredRoles()
        {
            return _roles.Values.Select(e => e.Source).ToList();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleDefinitionAttribute : Attribute
    {
        public RoleDefinitionAttribute() { }
    }

    public interface ICustomRole
    {
        string RoleName { get; }
        string RoleDesc { get; }
        string RoleRevealPrompt { get; }

        RoleData BuildRoleData();
    }
}