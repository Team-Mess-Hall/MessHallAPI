using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using MessHallAPI.Managers.Cosmetic;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class ReportBodyPatch
    {
        public static void Postfix(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            CustomNameplateManager.RefreshMeetingAtlases();
        }
    }
    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new Type[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
    public class CallEmergencyMeetingPatch
    {
        public static void Postfix(PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            CustomNameplateManager.RefreshMeetingAtlases();
        }
    }
}
