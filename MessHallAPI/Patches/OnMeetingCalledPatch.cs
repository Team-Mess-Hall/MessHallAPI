using Fusion;
using HarmonyLib;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Managers.Cosmetic;
using MessHallAPI.Managers.Role;
using MessHallAPI.Networking;
using SG.Airlock;
using SG.Airlock.Localization;
using SG.Airlock.Roles;
using System.Collections;
using UnityEngine;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class ReportBodyPatch
    {
        public static void Postfix(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            CoreBehaviour.Instance.StartCoroutine(DelayedRefresh.Run().ToString());
        }

        public static void Prefix(VoteManager __instance, int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            Meetinghandler.HandleBodyCalled(__instance, foundPlayer, sourcePlayer, forceVote, info);
        }
    }

    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class CallEmergencyMeetingPatch
    {
        public static void Postfix(PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            CoreBehaviour.Instance.StartCoroutine(DelayedRefresh.Run().ToString());
        }

        public static void Prefix(VoteManager __instance, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            Meetinghandler.HandleEmergencyMeetingCalled(__instance, sourcePlayer, forceVote, info);
        }
    }

    internal static class DelayedRefresh
    {
        internal static IEnumerator Run()
        {
            yield return new WaitForSeconds(3f);
            if (Core.SceneName == "Skeld")
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_RefreshSkeldMeetingAtlases");
            }
            else if (Core.SceneName == "PolusPoint")
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_RefreshPolusPointMeetingAtlases");
            }
            else if (Core.SceneName == "MessHall")
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_RefreshMessHallMeetingAtlases");
            }
        }
    }

    public class Meetinghandler
    {
        public static void HandleEmergencyMeetingCalled(VoteManager __instance, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            if (Settings.IsHost)
            {
                foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
                {
                    GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                    NetworkManager.InvokeRPC("MessHallAPI", "RPC_EmergencyMeetingCalled", playerState.PlayerId, (int)trueRoleHost, sourcePlayer.PlayerId, forceVote);
                }
            }
        }

        public static void HandleBodyCalled(VoteManager __instance, int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            if (Settings.IsHost)
            {
                foreach (PlayerState playerState in References.Spawn.ActivePlayerStates)
                {
                    GameRole trueRoleHost = CustomRoleManager.GetTrueRoleHost(playerState.PlayerId);
                    NetworkManager.InvokeRPC("MessHallAPI", "RPC_BodyCalled", playerState.PlayerId, (int)trueRoleHost, foundPlayer, sourcePlayer.PlayerId, forceVote);
                }
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_EmergencyMeetingCalled([RPCTarget] int target, int gameRole, int sourcePlayer, bool forceVote)
        {
            ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;      
            if (CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple))
            {
                valueTuple.Item5.OnEmergencyMeetingCalled(sourcePlayer, forceVote);
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_BodyCalled([RPCTarget] int target, int gameRole, int foundPlayer, int sourcePlayer, bool forceVote)
        {
            ValueTuple<RoleData, string, string, string, ICustomRole, TextKey> valueTuple;
            if (CustomRoleManager._roles.TryGetValue((GameRole)gameRole, out valueTuple))
            {
                valueTuple.Item5.OnBodyCalled(foundPlayer, sourcePlayer, forceVote);
            }
        }
    }
}