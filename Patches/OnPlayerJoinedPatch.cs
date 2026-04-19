using HarmonyLib;
using Il2CppFusion;
using MelonLoader;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MessHallAPI.Patches
{
    internal class OnPlayerJoinedPatch
    {
        public static readonly Dictionary<int, string> ReliableKeys = new();
        public static readonly HashSet<int> Confirmed = new();

        public static bool TryGetKey(int id, out string key)
        {
            return ReliableKeys.TryGetValue(id, out key);
        }

        [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_PlayerJoined))]
        private static class Patch
        {
            [HarmonyPostfix]
            private static void Postfix(PlayerRef player)
            {
                if (!Settings.IsHost)
                    return;

                int id = player.PlayerId;

                string key = Guid.NewGuid().ToString();
                ReliableKeys[id] = key;

                if (id == References.networkRunner.LocalPlayer)
                {
                    RPCRegistry.ReliableKey = key;
                    Confirmed.Add(id);
                    return;
                }

                MelonCoroutines.Start(SendKeyLoop(id));
            }
        }

        private static IEnumerator SendKeyLoop(int playerId)
        {
            int attempts = 0;

            yield return new WaitForSeconds(0.5f);

            while (!Confirmed.Contains(playerId) && attempts < 6)
            {
                if (!ReliableKeys.TryGetValue(playerId, out var key))
                    yield break;

                Logging.Log($"send key to {playerId}");

                NetworkManager.InvokeRPC("MessHallAPI", "RPC_ExchangeKeys", playerId, key);

                attempts++;

                yield return new WaitForSeconds(1.5f);
            }

            if (!Confirmed.Contains(playerId))
            {
                Logging.Warn($"key exchange failed for {playerId}");
            }
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly)]
        public static void RPC_ExchangeKeys([RPCTarget] int target, string key)
        {
            int localId = References.Client.PState.PlayerId;

            if (localId != target)
                return;

            ReliableKeys[localId] = key;
            RPCRegistry.ReliableKey = key;

            Logging.Log("received key");

            NetworkManager.InvokeRPC("MessHallAPI", "RPC_KeyReceived");
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void RPC_KeyReceived([RPCInfo] MessHallRpcInfo info)
        {
            if (!Settings.IsHost)
                return;

            int senderId = info.SenderId;

            if (!ReliableKeys.ContainsKey(senderId))
                return;

            Confirmed.Add(senderId);

            Logging.Log($"key exchange confirmed from {senderId}");
        }
    }
}