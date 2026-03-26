using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppFusion;
using MelonLoader;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Networking;
using UnityEngine;

namespace MessHallAPI.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_PlayerJoined))]
    internal class OnPlayerJoinedPatch
    {
        public static readonly Dictionary<int, string> Keys = new();

        private static void Postfix(NetworkRunner __instance, PlayerRef player)
        {
            Settings.IsHost = __instance.LocalPlayer.PlayerId == 9;
            if (Settings.IsHost) MelonCoroutines.Start(AssignRpcKey(__instance, player));
        }

        private static IEnumerator AssignRpcKey(NetworkRunner runner, PlayerRef player)
        {
            yield return new WaitForSeconds(8f);

            if (runner == null || !runner.IsPlayerValid(player))
                yield break;

            var key = System.Guid.NewGuid().ToString();

            Keys[player.PlayerId] = key;

            var data = new Dictionary<string, object>
            {
                { "RpcName", "KeyExchange" },
                { "Key", key },
                { "UnreliableActor", runner.LocalPlayer.PlayerId }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            byte[] payload = new byte[jsonBytes.Length + 1];
            payload[0] = PacketConstants.MHAPI;
            Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);

            NetworkSender.SendToPlayer(player, payload);

            MelonLogger.Msg($"[rpc] sent key to {player.PlayerId}: {key}");
        }
    }
}