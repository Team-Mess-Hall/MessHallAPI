using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static MessHallAPI.Base.References;
using MessHallAPI.Debugger;
using Il2CppSG.Airlock.Network;
using MelonLoader;

namespace MessHallAPI.Networking
{
    internal static class NetworkSender
    {
        /// <summary>
        /// Send to a specific player.
        /// </summary>
        internal static void SendToPlayer(PlayerRef target, byte[] payload)
        {
            var runner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
            networkRunner = runner;

            runner.SendReliableDataToPlayer(target, new Il2CppStructArray<byte>(Wrap(payload)));
            MelonLogger.Msg($"Null: {networkRunner == null}, 2null: {runner == null}");

        }

        /// <summary>
        /// Send to the host/server.
        /// </summary>
        internal static void SendToServer(byte[] payload)
        {
            var runner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
            networkRunner = runner;

            networkRunner.SendReliableDataToServer(new Il2CppStructArray<byte>(Wrap(payload)));
            MelonLogger.Msg($"Null: {networkRunner == null}, 2null: {runner == null}");

        }

        /// <summary>
        /// Broadcast to all players individually.
        /// </summary>
        internal static void SendToAll(byte[] payload, bool includeLocal = false)
        {

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