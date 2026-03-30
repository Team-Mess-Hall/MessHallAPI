using HarmonyLib;
using Il2CppFusion;
using Il2CppSystem;
using MelonLoader;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Il2CppFusion.Simulation;

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

        private static IEnumerator ExchangeKeys(int playerId)
        {
            yield return new WaitForSeconds(9f);

            string guid = System.Guid.NewGuid().ToString();

            if (playerId == 9)
            {
                ReliableKeys[playerId] = guid;
                RPCRegistry.ReliableKey = guid;
                Logging.Log($"Host player joined, set key: {guid}");
            }
            else
            {
                ReliableKeys[playerId] = guid;
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_ExchangeKeys", playerId, guid);
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_ExchangeKeys([RPCTarget] int target, string key)
        {
            if (OnPlayerJoinedPatch.ReliableKeys.TryGetValue(target, out var existing))
            {
                if (existing == key) return;
            }

            OnPlayerJoinedPatch.ReliableKeys[target] = key;
            RPCRegistry.ReliableKey = key;

            Logging.Log($"Stored key for player {target}: {key}");
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_KeyReceived() { }
    }
}