using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Role;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndGame))]
    public class EndGamePatch
    {
        public static void Postfix(GameStateManager __instance, GameTeam winningTeam)
        {
            CustomRoleManager.ClearRoles();
        }
    }
}
