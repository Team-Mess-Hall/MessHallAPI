using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Patches;
using System.Text.Json;
using static MessHallAPI.Base.References;
using static MessHallAPI.Networking.RPCRegistry;

namespace MessHallAPI.Networking
{
    public static class NetworkManager
    {
        public static void InvokeRPC(string modName, string methodName, params object[] args)
        {
            if (!Settings.InGame)
                return;

            string key = modName + "::" + methodName;

            if (!RPCRegistry.TryGet(key, out var entry))
            {
                Logging.Log($"rpc not registered {key}");
                return;
            }

            int actorId = Client?.PState?.PlayerId ?? -1;

            if (entry.Attr.Caller == RPCCaller.HostOnly && !Settings.IsHost)
            {
                Logging.Log("rpc blocked hostonly");
                return;
            }

            int rpcTarget = -1;

            var parms = entry.Method.GetParameters();
            for (int i = 0; i < parms.Length && i < args.Length; i++)
            {
                if (Attribute.IsDefined(parms[i], typeof(RPCTargetAttribute)))
                {
                    if (args[i] is int t)
                        rpcTarget = t;
                    break;
                }
            }




            if (rpcTarget == networkRunner.LocalPlayer)
            {
                Logging.Log($"target {rpcTarget} is host");

                if (Settings.IsHost)
                    ExecuteLocal(entry, args);

                return;
            }

            RPCPacket packet = new RPCPacket
            {
                ModId = modName,
                ReliableKey = RPCRegistry.ReliableKey,
                Method = methodName,
                ActorId = actorId,
                Args = args
            };

            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(packet);

            if (!Settings.IsHost)
            {
                Logging.Log($"sending rpc {methodName} to host");

                NetworkSender.SendToServer(bytes);

                if (entry.Attr.Target == RPCTarget.AllInclusive)
                    ExecuteLocal(entry, args);

                return;
            }
            if (entry.Attr.Target == RPCTarget.Host)
            {
                Logging.Log($"host executing {packet.Method}");
                ExecuteLocal(entry, args);
                return;
            }

            if (entry.Attr.Target == RPCTarget.AllInclusive)
            {
                Logging.Log($"host executing {packet.Method}");
                ExecuteLocal(entry, args);
            }

            NetworkSender.RelayToTargets(entry, rpcTarget, packet, actorId);
        }

        public static void OperationReceived(PlayerRef sender, Il2CppStructArray<byte> dataArray)
        {
            if (!Settings.InGame)
                return;

            try
            {
                byte[] data = dataArray;

                if (data == null || data.Length < 2)
                    return;

                if (data[0] != PacketConstants.MHAPI)
                    return;

                var span = new ReadOnlySpan<byte>(data, 1, data.Length - 1);
                RPCPacket? packet = JsonSerializer.Deserialize<RPCPacket>(span);

                if (packet == null)
                    return;

                string key = packet.ModId + "::" + packet.Method;

                if (!RPCRegistry.TryGet(key, out var entry))
                    return;

                Logging.Log($"[RPC] Received {key} from {packet.ActorId}");

                object[] raw = packet.Args ?? Array.Empty<object>();
                object[] rpcArgs = raw.Length == 0 ? raw : new object[raw.Length];

                for (int i = 0; i < raw.Length; i++)
                {
                    object value = raw[i];

                    if (value is JsonElement element)
                    {
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.String:
                                value = element.GetString();
                                break;

                            case JsonValueKind.Number:
                                if (element.TryGetInt32(out int iVal))
                                    value = iVal;
                                else if (element.TryGetSingle(out float fVal))
                                    value = fVal;
                                else
                                    value = (float)element.GetDouble();
                                break;

                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                value = element.GetBoolean();
                                break;

                            default:
                                value = element.ToString();
                                break;
                        }
                    }

                    rpcArgs[i] = value;
                }

                if (Settings.IsHost)
                {
                    int realSender = sender.PlayerId;
                    int claimedSender = packet.ActorId;

                    Logging.Log($"host got rpc {packet.Method} from {realSender}");

                    bool senderIsHost = realSender == networkRunner.LocalPlayer;

                    if (!senderIsHost && realSender != claimedSender)
                    {
                        Logging.Warn($"spoof blocked real {realSender} fake {claimedSender}");
                        return;
                    }

                    if (entry.Attr.Caller == RPCCaller.HostOnly && realSender != networkRunner.LocalPlayer)
                    {
                        Logging.Warn($"client sided blocked hostonly rpc from {realSender}");
                        return;
                    }

                    if (packet.Method != "RPC_ExchangeKeys")
                    {
                        if (!OnPlayerJoinedPatch.TryGetKey(realSender, out var expectedKey))
                        {
                            Logging.Warn($"no key for {realSender}");
                            return;
                        }

                        if (packet.ReliableKey != expectedKey)
                        {
                            Logging.Warn($"bad key from {realSender}");
                            return;
                        }

                        Logging.Log($"verified sender {realSender}");
                    }
                    else
                    {
                        Logging.Log($"key exchange OK from {realSender}");
                    }

                    int rpcTarget = -1;

                    var parms = entry.Method.GetParameters();
                    for (int i = 0; i < parms.Length && i < rpcArgs.Length; i++)
                    {
                        if (Attribute.IsDefined(parms[i], typeof(RPCTargetAttribute)))
                        {
                            if (rpcArgs[i] is int t)
                                rpcTarget = t;
                            break;
                        }
                    }

                    if (entry.Attr.Target == RPCTarget.Host ||
                        entry.Attr.Target == RPCTarget.AllInclusive)
                    {
                        Logging.Log($"host executing {packet.Method}");
                        ExecuteLocal(entry, rpcArgs);
                    }

                    Logging.Log($"relaying {packet.Method} from {realSender}");

                    NetworkSender.RelayToTargets(entry, rpcTarget, packet, realSender);
                }
                else
                {
                    int localId = Client?.PState?.PlayerId ?? -1;

                    bool execute = false;

                    int rpcTarget = -1;

                    var parms = entry.Method.GetParameters();
                    for (int i = 0; i < parms.Length && i < rpcArgs.Length; i++)
                    {
                        if (Attribute.IsDefined(parms[i], typeof(RPCTargetAttribute)))
                        {
                            if (rpcArgs[i] is int t)
                                rpcTarget = t;
                            break;
                        }
                    }

                    if (rpcTarget != -1)
                    {
                        execute = localId == rpcTarget;
                    }
                    else
                    {
                        if (entry.Attr.Target == RPCTarget.All)
                            execute = localId != packet.ActorId;

                        if (entry.Attr.Target == RPCTarget.AllInclusive)
                            execute = true;
                    }

                    if (execute)
                    {
                        Logging.Log($"client exec {packet.Method}");
                        ExecuteLocal(entry, rpcArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log($"[RPC] Error: {ex}");
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