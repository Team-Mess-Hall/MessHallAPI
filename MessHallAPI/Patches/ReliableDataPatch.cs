using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MessHallAPI.Networking;
using HarmonyLib;
using MessHallAPI.Debugger;
using Fusion;

namespace MessHallAPI.Patches
{
    [HarmonyPatch]
    internal static class ReliableDataPatch
    {
        [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_OnReliableData))]
        [HarmonyPostfix]
        private static void Postfix(PlayerRef player, Il2CppStructArray<byte> dataArray)
        {
            try
            {
                NetworkManager.OperationReceived(player, dataArray);
            }
            catch { }
        }
    }
}