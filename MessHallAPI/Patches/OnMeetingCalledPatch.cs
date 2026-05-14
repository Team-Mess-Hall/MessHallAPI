using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using MessHallAPI.Managers.Cosmetic;
using MelonLoader;
using System.Collections;
using UnityEngine;
using MessHallAPI.Networking;
using MessHallAPI.Base;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class ReportBodyPatch
    {
        public static void Postfix(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            MelonCoroutines.Start(DelayedRefresh.Run());
        }
    }

    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class CallEmergencyMeetingPatch
    {
        public static void Postfix(PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            MelonCoroutines.Start(DelayedRefresh.Run());
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
}