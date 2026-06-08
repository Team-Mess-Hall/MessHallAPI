using Fusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SG.Airlock.Network;
using Il2CppSystem.IO;
using MessHallAPI.Debugger;
using MessHallAPI.Patches;
using System.Text.Json;
using UnityEngine.Playables;
using static MessHallAPI.Base.References;
using static MessHallAPI.Networking.RPCRegistry;

namespace MessHallAPI.Networking
{
    internal static class NetworkSender
    {
        /// <summary>
        /// Send to a specific player.
        /// </summary>
        internal static void SendToPlayer(PlayerRef target, byte[] payload)
        {
            if (networkRunner == null && target.IsValid())
            {
                Logging.Error("NetworkRunner not available.");
                networkRunner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
                Logging.DebugLog($"Fixed? {networkRunner != null}");
            }

            var arr = new Il2CppStructArray<byte>(payload.Length);
            for (int i = 0; i < payload.Length; i++)
                arr[i] = payload[i];

            networkRunner.SendReliableDataToPlayer(target, arr);
        }

        /// <summary>
        /// Send to the host/server.
        /// </summary>
        internal static void SendToServer(byte[] payload)
        {
            if (networkRunner == null)
            {
                Logging.Error("NetworkRunner not available.");
                networkRunner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
                Logging.DebugLog($"Fixed? {networkRunner != null}");
            }

            var arr = new Il2CppStructArray<byte>(payload.Length);
            for (int i = 0; i < payload.Length; i++)
                arr[i] = payload[i];

            networkRunner.SendReliableDataToServer(arr);
        }
    }
}