using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;

[HarmonyPatch(typeof(NetworkedKillBehaviour), "RPC_TargetedAction")]
public class TargetedActionPatch
{
    public static void Postfix(NetworkedKillBehaviour __instance, PlayerRef targetedPlayer, PlayerRef perpetrator, int action)
    {
        if (Settings.IsHost)
        {
            foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
            {
                GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_TargetedAction", playerState.PlayerId, (int)trueRoleHost, targetedPlayer.PlayerId, perpetrator.PlayerId, action);
            }
        }
    }

    [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
    public static void RPC_TargetedAction([RPCTarget] int target, int gameRole, int targetedPlayer, int perpetrator, int action)
    {
        ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;
        if (CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple))
        {
            valueTuple.Item5.OnTargetedAction(targetedPlayer, perpetrator, action);
        }
    }
}