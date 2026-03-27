using System.Numerics;
using System.Text.Json;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using MelonLoader;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Patches;
using UnityEngine.Playables;
using static MessHallAPI.Base.References;
using static MessHallAPI.Networking.RPCRegistry;

namespace MessHallAPI.Networking
{
    public static class NetworkManager
    {

        public static void InvokeRPC(string modId, string methodName, params object[] args)
        {
            if (!Settings.InGame)
            {
                Logging.Warn("[RPC] Not in game, exiting early.");
                return;
            }

            string key = modId + "::" + methodName;

            if (!RPCRegistry.TryGet(key, out var entry))
            {
                Logging.Error($"[RPC] {key} is not registered!");
                return;
            }

            if (entry.Attr.Caller == RPCCaller.HostOnly && !Settings.IsHost)
            {
                Logging.DebugWarn($"[RPC] HostOnly Rpc {key} blocked");
                return;
            }

            try
            {
                int actorId = Client?.PState?.PlayerId ?? -1;

                int RpcTarget = -1;

                var parms = entry.Method.GetParameters();
                for (int i = 0; i < parms.Length && i < args.Length; i++)
                {
                    if (Attribute.IsDefined(parms[i], typeof(RPCTargetAttribute)))
                    {
                        if (args[i] is int TargetRef)
                            RpcTarget = TargetRef;
                        break;
                    }
                }

                RPCPacket packet = new RPCPacket
                {
                    ModId = modId,
                    Method = methodName,
                    ActorId = actorId,
                    ReliableKey = (entry.Attr.Target == RPCTarget.Host || (Settings.IsHost && RpcTarget != -1)) ? RPCRegistry.ReliableKey : "",
                    Args = args
                };

                Logging.DebugLog($"[RPC] Invoking {key} with {packet.Args.Length} args");

                byte[] JsonBytes = JsonSerializer.SerializeToUtf8Bytes(packet);

                if (RpcTarget != -1)
                {
                    NetworkSender.SendToPlayer(RpcTarget, JsonBytes);
                    return;
                }

                switch (entry.Attr.Target)
                {
                    case RPCTarget.InputAuthority:
                        NetworkSender.SendToPlayer(networkRunner.LocalPlayer, JsonBytes);
                        break;

                    case RPCTarget.Host:
                        if (Settings.IsHost)
                            ExecuteLocal(entry, args);
                        else
                            NetworkSender.SendToServer(JsonBytes);
                        break;

                    case RPCTarget.All:
                        NetworkSender.SendToAll(JsonBytes, false);
                        break;

                    case RPCTarget.AllInclusive:
                        ExecuteLocal(entry, args);
                        NetworkSender.SendToAll(JsonBytes, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"[RPC] Error: {ex}");
            }
        }
        public static void OperationReceived(PlayerRef sender, Il2CppStructArray<byte> dataArray)
        {
            if (!Settings.InGame) return;

            try
            {
                byte[] data = dataArray;
                if (data == null || data.Length < 2) return;

                if (data[0] != PacketConstants.MHAPI) return;

                var span = new ReadOnlySpan<byte>(data, 1, data.Length - 1);
                RPCPacket? packet = JsonSerializer.Deserialize<RPCPacket>(span);
                if (packet == null) return;

                if (Settings.IsHost)
                {
                    string? ClaimedKey = packet.ReliableKey;
                    int actor = packet.ActorId;

                    if (string.IsNullOrEmpty(ClaimedKey))
                    {
                        Logging.Warn($"[RPC] Missing key from {actor}");
                        return;
                    }

                    if (!OnPlayerJoinedPatch.TryGetKey(actor, out var Token))
                    {
                        Logging.Warn($"[RPC] No stored key for {actor}");
                        return;
                    }

                    if (Token != ClaimedKey)
                    {
                        Logging.Warn($"[RPC] Key mismatch for {actor} | expected: {Token} got: {ClaimedKey}");
                        return;
                    }

                    Logging.Log($"[RPC] OK for {actor}");
                }

                string key = packet.ModId + "::" + packet.Method;
                
                if (!RPCRegistry.TryGet(key, out var entry)) return;

                if (entry.Attr.Target == RPCTarget.Host && !Settings.IsHost)
                {
                    Logging.DebugWarn($"Host targeted Rpc {key} blocked");
                    return;
                }

                Logging.DebugLog($"[RPC] Executing {key} from {packet.ActorId}");

                object[] raw = packet.Args ?? Array.Empty<object>();
                object[] RpcArgs = raw.Length == 0 ? raw : new object[raw.Length];

                for (int i = 0; i < raw.Length; i++)
                {
                    object value = raw[i];

                    if (value is JsonElement element)
                    {
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.String: value = element.GetString(); break;
                            case JsonValueKind.Number: value = element.GetInt32(); break;
                            case JsonValueKind.True:
                            case JsonValueKind.False: value = element.GetBoolean(); break;
                            default: value = element.ToString(); break;
                        }
                    }

                    RpcArgs[i] = value;
                }

                ExecuteLocal(entry, RpcArgs);
            }
            catch (Exception ex)
            {
                Logging.Error($"[RPC] Error: {ex}");
            }
        }


        private static void ExecuteLocal(RPCRegistry.RPCEntry entry, object[]? args)
        {
            try
            {
                entry.Method.Invoke(entry.Owner, args);
            }
            catch (Exception ex)
            {
                Logging.Error($"RPC error in {entry.Method.Name}: {ex}");
            }
        }
    }

    internal static class PacketConstants
    {
        public const byte MHAPI = 0x4d;
    }
}