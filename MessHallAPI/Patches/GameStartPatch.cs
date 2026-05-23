using HarmonyLib;
using Il2CppSG.Airlock;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Role;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.StartGame))]
    public class GameStartPatch
    {
        public static void Prefix()
        {
            if (Settings.IsHost)
            { 
                if (roleManager?.gameRoleToPlayerIds == null) return;

                foreach (var (role, _) in CustomRoleManager._roles)
                {
                    if (!roleManager.gameRoleToPlayerIds.ContainsKey(role))
                    {
                        roleManager.gameRoleToPlayerIds[role] = new Il2CppSystem.Collections.Generic.List<int>();
                        Logging.Log($"CustomRoleManager: Re-injected '{role}' into gameRoleToPlayerIds");
                    }
                }
            }
        }
    }
}
