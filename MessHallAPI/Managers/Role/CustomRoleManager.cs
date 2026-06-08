using BepInEx.Unity.IL2CPP;
using Fusion;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using SG.Airlock.Localization;
using SG.Airlock.Roles;
using SG.GlobalEvents.Variables;
using System.Reflection;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.Role
{
    public static class CustomRoleManager
    {
        public static readonly Dictionary<GameRole, (RoleData Definition, string RoleName, string RoleDescription, string RoleRevealPrompt, ICustomRole Source, TextKey DescKey)> _roles = new();
        private static readonly List<(GameRole Role, ICustomRole Source)> _pendingRoles = new();
        private static readonly Dictionary<int, int> _roleCache = new();
        private static readonly Dictionary<Type, GameRole> _typeToRole = new();

        private const int VanillaRoleMax = 10;
        private static int _nextRoleId = VanillaRoleMax + 1;

        public static GameRole assignedClientRole = GameRole.NotSet;

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

        public static void AutoRegisterRoles()
        {
            foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
            {
                if (pluginInfo.Instance == null) continue;

                foreach (Type type in pluginInfo.Instance.GetType().Assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<RoleDefinitionAttribute>() == null) continue;

                    if (!typeof(ICustomRole).IsAssignableFrom(type))
                    {
                        Logging.Warn($"{type.Name} has [RoleDefinition] but does not implement ICustomRole.");
                        continue;
                    }

                    var role = (ICustomRole)Activator.CreateInstance(type);
                    Register(AllocateRole(), role);
                }
            }
        }

        public static void FlushRoles()
        {
            try
            {
                if (roleManager == null || roleManager._availableRoles == null) return;

                foreach (var (role, source) in _pendingRoles)
                {
                    var data = source.BuildRoleData();
                    data.Role = role;

                    data.RoleNameTK = CreateTextKey();
                    data.RolePromptTK = CreateTextKey();
                    data.EjectionReveal = CreateTextKey();
                    data.ChanceOfRoleAssignment = CreateRoleFloatVar();
                    data.MaxNumOfRole = CreateRoleIntVar();

                    if (data._targetedAction != ProximityTargetedAction.None)
                        data._targetedActionCooldown = CreateRoleCooldownIntVar();

                    RegisterTextKey(source.RoleName, data.RoleNameTK);
                    RegisterTextKey(source.RoleRevealPrompt, data.RolePromptTK);
                    RegisterTextKey(
                        $"was a <color=#{ColorToHex(data.RoleNameColor)}>{source.RoleName}</color>.",
                        data.EjectionReveal);

                    if (!roleManager._availableRoles.AllRoles.Contains(data))
                        roleManager._availableRoles.AllRoles.Add(data);

                    roleManager._availableRoles._filteredAvailableRoles.Add(data);

                    if (roleManager.gameRoleToPlayerIds != null &&
                        !roleManager.gameRoleToPlayerIds.ContainsKey(role))
                    {
                        roleManager.gameRoleToPlayerIds[role] =
                            new Il2CppSystem.Collections.Generic.List<int>();
                        Logging.DebugLog($"CustomRoleManager: Initialized gameRoleToPlayerIds for {source.RoleName}");
                    }

                    var descKey = CreateTextKey();
                    RegisterTextKey(source.RoleDesc, descKey);
                    _roles[role] = (data, source.RoleName, source.RoleDesc, source.RoleRevealPrompt, source, descKey);
                    _typeToRole[source.GetType()] = role;
                }

                _pendingRoles.Clear();
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }

        public static bool IsCustomRole(GameRole role) => _roles.ContainsKey(role);

        public static GameRole GetRole<T>() where T : class
        {
            GameRole gameRole;
            return CustomRoleManager._typeToRole.TryGetValue(typeof(T), out gameRole) ? gameRole : 0;
        }

        public static IReadOnlyList<ICustomRole> GetRegisteredRoles() =>
            _roles.Values.Select(e => e.Source).ToList();

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void RPC_SendRoleToAll(int playerID, int gameRole)
        {
            var role = (GameRole)gameRole;
            _roleCache[playerID] = gameRole;
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void RPC_SendRole([RPCTarget] int playerID, int gameRole)
        {
            var role = (GameRole)gameRole;

            if (Client.PState != null && playerID == Client.PState.PlayerId)
            {
                assignedClientRole = role;
                if (_roles.TryGetValue(role, out var entry))
                    entry.Source.OnRoleRevealed(role);
            }

            Logging.DebugLog($"[RoleSync] Received role {role} for player {playerID}");
        }

        public static void GetTrueRole(int playerID)
        {
            foreach (var roleEntry in roleManager.gameRoleToPlayerIds)
            {
                foreach (int id in roleEntry.Value)
                {
                    if (id != playerID) continue;
                    NetworkManager.InvokeRPC("MessHallAPI", "RPC_SendRoleToAll", playerID, (int)roleEntry.Key);
                    return;
                }
            }
        }

        public static GameRole GetTrueRoleHost(int playerID)
        {
            foreach (var roleEntry in roleManager.gameRoleToPlayerIds)
                foreach (int id in roleEntry.Value)
                    if (id == playerID) return roleEntry.Key;
            return GameRole.NotSet;
        }

        public static bool TryGetCachedRole(int playerID, out int role) =>
            _roleCache.TryGetValue(playerID, out role);

        public static void Unregister(GameRole role)
        {
            if (!_roles.TryGetValue(role, out var entry)) return;
            roleManager?._availableRoles?.AllRoles.Remove(entry.Definition);
            _roles.Remove(role);
            Logging.DebugLog($"CustomRoleManager: Unregistered role {role}");
        }

        public static void ClearRoles()
        {
            foreach (var roleEntry in roleManager.gameRoleToPlayerIds)
            {
                roleEntry.Value.Clear();
                roleEntry.value.Clear();
            }
        }

        public static TextKey CreateTextKey() =>
            UnityEngine.Object.Instantiate(roleManager._availableRoles.AllRoles[3].EjectionReveal);

        public static FloatVariable CreateRoleFloatVar() =>
            UnityEngine.Object.Instantiate(roleManager._availableRoles.AllRoles[3].ChanceOfRoleAssignment);

        public static IntVariable CreateRoleIntVar() =>
            UnityEngine.Object.Instantiate(roleManager._availableRoles.AllRoles[3].MaxNumOfRole);

        public static IntVariable CreateRoleCooldownIntVar() =>
            UnityEngine.Object.Instantiate(roleManager._availableRoles.AllRoles[1]._targetedActionCooldown);

        public static void RegisterTextKey(string text, TextKey textKey)
        {
            var db = roleManager._availableRoles.AllRoles[0].EjectionReveal._localizationManager._currentTextDatabase;
            db.AddNewEntry(textKey, text);
        }

        public static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleDefinitionAttribute : Attribute { }

    public interface ICustomRole
    {
        string RoleName { get; }
        string RoleDesc { get; }
        string RoleRevealPrompt { get; }
        Sprite RoleIcon { get; }
        RoleData BuildRoleData();

        void OnRoleRevealed(GameRole role) { }
        void OnEmergencyMeetingCalled(PlayerRef sourcePlayer, NetworkBool forceVote) { }
        void OnBodyCalled(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote) { }
        void OnPlayerWasKilled(PlayerRef victim, PlayerRef killer, bool vigiKill, bool selfKill) { }
        void OnEndGame(GameTeam winningTeam) { }
        void OnTargetedAction(int target, int perp, int action) { }
        void OnEndVote();
    }
}