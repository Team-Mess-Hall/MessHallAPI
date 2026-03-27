using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static MessHallAPI.Base.References;
using MessHallAPI.Debugger;
using Il2CppSG.Airlock.Network;

namespace MessHallAPI.Networking
{
    internal static class NetworkSender
    {
        /// <summary>
        /// Send to a specific player.
        /// </summary>
        internal static void SendToPlayer(PlayerRef target, byte[] payload)
        {
            if (networkRunner == null && target.IsValid)
            {
                Logging.Error("NetworkRunner not available.");
                networkRunner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
                Logging.Log($"Fixed? {networkRunner != null}");

                return;
            }

            networkRunner.SendReliableDataToPlayer(target, new Il2CppStructArray<byte>(Wrap(payload)));
        }

        /// <summary>
        /// Send to the host/server.
        /// </summary>
        internal static void SendToServer(byte[] payload)
        {
            if (networkRunner == null)
            {
                Logging.Error("NetworkRunner not available.");
                return;
            }

            networkRunner.SendReliableDataToServer(new Il2CppStructArray<byte>(Wrap(payload)));
        }

        /// <summary>
        /// Broadcast to all players individually.
        /// </summary>
        internal static void SendToAll(byte[] payload, bool includeLocal = false)
        {
            if (networkRunner == null) return;

            foreach (var playerState in Spawn.ActivePlayerStates)
            {
                if (!includeLocal && playerState.PlayerId == networkRunner.LocalPlayer)
                    continue;

                SendToPlayer(playerState.PlayerId, payload);
            }
        }



        private static byte[] Wrap(byte[] payload)
        {
            var result = new byte[payload.Length + 1];
            result[0] = PacketConstants.MHAPI;
            Buffer.BlockCopy(payload, 0, result, 1, payload.Length);
            return result;
        }
    }
}