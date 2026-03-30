using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock.Network;
using MelonLoader;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Cosmetic;
using MessHallAPI.Networking;
using UnityEngine;

namespace MessHallAPI.Patches
{
    internal class OnPlayerJoinedPatch
    {
        public static readonly Dictionary<int, string> ReliableKeys = new();

        public static bool TryGetKey(int id, out string? key)
        {
            return ReliableKeys.TryGetValue(id, out key);
        }

        [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_PlayerJoined))]
        private static class Patch
        {
            private static void Prefix(PlayerRef player)
            {
                if (!Settings.IsHost)
                    return;

                MelonCoroutines.Start(ExchangeKeys(player.PlayerId));
            }
        }

        [HarmonyPatch(typeof(SpawnManager), nameof(SpawnManager.OnPlayerJoined))]
        private static void Postfix(NetworkRunner runner, PlayerRef player)
        {
            CustomNameplateManager.RefreshPlayerAtlases();
        }

        private static IEnumerator ExchangeKeys(int playerId)
        {
            yield return new WaitForSeconds(9f);

            string guid = Guid.NewGuid().ToString();

            ReliableKeys[playerId] = guid;
            if (playerId == 9)
            {
                RPCRegistry.ReliableKey = guid;
            }
            else NetworkManager.InvokeRPC("MessHallAPI", "RPC_ExchangeKeys", playerId, guid);
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_ExchangeKeys([RPCTarget] int target, string Key)
        {
            if (string.IsNullOrEmpty(RPCRegistry.ReliableKey))
                RPCRegistry.ReliableKey = Key;
        }
    }
}