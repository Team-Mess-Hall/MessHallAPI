// Token: 0x02000014 RID: 20
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;

[HarmonyPatch(typeof(PlayerState),nameof(PlayerState.RPC_ShowDeathAnim))]
public class ShowDeathAnimPatch
{
    public static void Postfix(PlayerState __instance, PlayerRef victim, PlayerRef killer, bool wasVigilanteKill, bool wasSelfKill)
    {
        if (Settings.IsHost)
        {
            foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
            {
                GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_PlayerWasKilled", playerState.PlayerId, (int)trueRoleHost, victim.PlayerId, killer.PlayerId, wasVigilanteKill, wasSelfKill);
            }
        }
    }

    [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
    public static void RPC_PlayerWasKilled([RPCTarget] int target, int gameRole, int victim, int killer, bool wasVigilanteKill, bool wasSelfKill)
    {
        ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;
        bool flag = !CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple);
        if (!flag)
        {
            valueTuple.Item5.OnPlayerWasKilled(victim, killer, wasVigilanteKill, wasSelfKill);
        }
    }
}