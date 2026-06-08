using HarmonyLib;
using Fusion;
using SG.Airlock;
using SG.Airlock.Localization;
using SG.Airlock.Roles;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndGame))]
    public class EndGamePatch
    {
        public static void Postfix(GameStateManager __instance, GameTeam winningTeam)
        {
            CustomRoleManager.ClearRoles();
        }
        public static void Prefix(GameStateManager __instance, GameTeam winningTeam)
        {
            foreach (var role in CustomRoleManager._roles)
            {
                role.Value.Source.OnEndGame(winningTeam);
            }
        }
    }

    public class EndGameHandler
    {
        public static void EndGame(GameTeam winningTeam)
        {
            bool flag = !Settings.IsHost;
            if (!flag)
            {
                foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
                {
                    GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                    NetworkManager.InvokeRPC("MessHallAPI", "RPC_EndGame", playerState.PlayerId, (int)trueRoleHost, (int)winningTeam);
                }
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_EndGame([RPCTarget] int Target, int gameRole, int winningTeam)
        {
            ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;
            if (CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple))
            {
                valueTuple.Item5.OnEndGame((GameTeam)winningTeam);
            }
        }
    }
}
