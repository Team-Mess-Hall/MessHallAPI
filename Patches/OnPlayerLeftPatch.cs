using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppFusion;
using MelonLoader;
using MessHallAPI.Base;
using MessHallAPI.Networking;
using UnityEngine;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_PlayerLeft))]
    internal class OnPlayerLeft
    {
        private static void Postfix(NetworkRunner __instance, PlayerRef player)
        {
            if (OnPlayerJoinedPatch.Keys.ContainsKey(player))
            {
                OnPlayerJoinedPatch.Keys.Remove(player);
                MelonLogger.Msg($"Removed key for {player}");
            }
        }
    }
}