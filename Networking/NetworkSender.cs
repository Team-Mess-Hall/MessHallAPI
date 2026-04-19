using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock.Network;
using Il2CppSystem.IO;
using MessHallAPI.Debugger;
using MessHallAPI.Patches;
using System.Text.Json;
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
            if (networkRunner == null && target.IsValid)
            {
                Logging.Error("NetworkRunner not available.");
                networkRunner = UnityEngine.Object.FindObjectOfType<AirlockNetworkRunner>();
                Logging.Log($"Fixed? {networkRunner != null}");
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
                Logging.Log($"Fixed? {networkRunner != null}");
            }

            var arr = new Il2CppStructArray<byte>(payload.Length);
            for (int i = 0; i < payload.Length; i++)
                arr[i] = payload[i];

            networkRunner.SendReliableDataToServer(arr);
        }

        /// <summary>
        /// Relay to all players individually from server
        /// </summary>
        internal static void RelayToTargets(RPCEntry entry, int rpcTarget, RPCPacket originalPacket, int senderId)
        {
            int hostId = networkRunner.LocalPlayer;

            foreach (PlayerRef player in networkRunner.ActivePlayers.ToArray())
            {

                bool send = false;

                if (rpcTarget != -1)
                {
                    send = player.PlayerId == rpcTarget && player.PlayerId != hostId;
                }
                else
                {
                    if (entry.Attr.Target == RPCTarget.All)
                        send = player.PlayerId != senderId && player.PlayerId != hostId;

                    if (entry.Attr.Target == RPCTarget.AllInclusive)
                        send = player.PlayerId != hostId;

                    if (entry.Attr.Target == RPCTarget.InputAuthority)
                        send = player.PlayerId == hostId;
                }

                if (!send)
                    continue;

                string key = "";

                if (OnPlayerJoinedPatch.TryGetKey(player.PlayerId, out var ReliableKey))
                    key = ReliableKey;

                RPCPacket packet = new RPCPacket
                {
                    ModId = originalPacket.ModId,
                    Method = originalPacket.Method,
                    ActorId = originalPacket.ActorId,
                    ReliableKey = key,
                    Args = originalPacket.Args
                };

                byte[] bytes = NetworkManager.Serialize(packet);
                SendToPlayer(player, bytes);
            }
        }


    }
}