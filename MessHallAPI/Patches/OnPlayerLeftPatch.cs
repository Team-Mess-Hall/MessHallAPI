using System;
using Fusion;
using HarmonyLib;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Cosmetic;

namespace MessHallAPI.Patches
{
    internal class OnPlayerLeftPatch
    {
        [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_PlayerLeft))]
        private static class Patch
        {
            private static void Prefix(PlayerRef player)
            {
                if (!Settings.IsHost)
                    return;

                OnPlayerJoinedPatch.ReliableKeys.Remove(player.PlayerId);
                Logging.DebugLog($"Player {player.PlayerId} left, removing their key.");

                CustomPetManager.OnPlayerLeft(player.PlayerId);
            }
        }
    }
}