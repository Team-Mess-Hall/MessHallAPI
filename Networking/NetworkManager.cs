using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using Il2CppSystem.IO;
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
                return;

            string rpcKey = modId + "::" + methodName;

            if (!RPCRegistry.TryGet(rpcKey, out var entry))
            {
                Logging.DebugLog("rpc not found " + rpcKey);
                return;
            }

            int actorId = Client.PState.PlayerId;

            if (entry.Attr.Caller == RPCCaller.HostOnly && !Settings.IsHost)
            {
                return;
            }
            string key = "";

            if (OnPlayerJoinedPatch.TryGetKey(actorId, out var k))
                key = k;

            RPCPacket packet = new RPCPacket
            {
                ModId = modId,
                Method = methodName,
                ActorId = actorId,
                ReliableKey = RPCRegistry.ReliableKey,
                Args = args
            };
            Logging.DebugLog($"invoke rpc {methodName} with key {key}");
            byte[] bytes = Serialize(packet);

            if (!Settings.IsHost)
            {
                NetworkSender.SendToServer(bytes);
                return;
            }


            var arr = new Il2CppStructArray<byte>(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
                arr[i] = bytes[i];

            OperationReceived(networkRunner.LocalPlayer, arr);
        }

        public static void OperationReceived(PlayerRef sender, Il2CppStructArray<byte> dataArray)
        {
            try
            {
                byte[] data = dataArray;

                if (data == null || data.Length < 2)
                    return;

                if (data[0] != PacketConstants.MHAPI)
                    return;

                if (data.Length > 1 && data[1] == PacketConstants.MHAPI)
                    return;

                ReadOnlySpan<byte> jsonSpan = new ReadOnlySpan<byte>(data, 1, data.Length - 1);

                RPCPacket packet = JsonSerializer.Deserialize<RPCPacket>(jsonSpan);
                if (packet == null)
                    return;

                string rpcKey = packet.ModId + "::" + packet.Method;

                if (!RPCRegistry.TryGet(rpcKey, out var entry))
                    return;

                object[] raw = packet.Args ?? Array.Empty<object>();
                object[] RpcArgs = new object[raw.Length];

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

                    RpcArgs[i] = value;
                }

                ParameterInfo[]? parms = entry.Method.GetParameters();

                int InfoIndex = -1;

                for (int i = 0; i < parms.Length; i++)
                {
                    if (Attribute.IsDefined(parms[i], typeof(RPCInfoAttribute)))
                    {
                        InfoIndex = i;
                        break;
                    }
                }

                object[] FinalOperationInfo;

                if (InfoIndex != -1)
                {
                    FinalOperationInfo = new object[parms.Length];

                    int index = 0;

                    for (int paremeter = 0; paremeter < parms.Length; paremeter++)
                    {
                        if (paremeter == InfoIndex)
                        {
                            FinalOperationInfo[paremeter] = new MessHallRpcInfo
                            {
                                SenderId = sender.PlayerId,
                                IsHost = Settings.IsHost
                            };
                        }
                        else
                        {
                            FinalOperationInfo[paremeter] = index < RpcArgs.Length ? RpcArgs[index++] : null;
                        }
                    }
                }
                else
                {
                    FinalOperationInfo = RpcArgs;
                }

                if (Settings.IsHost)
                {
                    int ReliableSender = sender.PlayerId;
                    int UnreliableSender = packet.ActorId;

                    if (ReliableSender != UnreliableSender)
                        return;

                    if (entry.Attr.Caller == RPCCaller.HostOnly && ReliableSender != networkRunner.LocalPlayer)
                        return;

                    if (packet.Method != "RPC_ExchangeKeys")
                    {
                        if (!OnPlayerJoinedPatch.TryGetKey(ReliableSender, out var expectedKey))
                            return;

                        if (string.IsNullOrEmpty(packet.ReliableKey))
                            return;

                        if (packet.ReliableKey != expectedKey)
                            return;
                    }

                    int rpcTarget = -1;

                    var methodParams = entry.Method.GetParameters();
                    for (int i = 0; i < methodParams.Length && i < RpcArgs.Length; i++)
                    {
                        if (Attribute.IsDefined(methodParams[i], typeof(RPCTargetAttribute)))
                        {
                            if (RpcArgs[i] is int t)
                                rpcTarget = t;
                            break;
                        }
                    }

                    if (rpcTarget != -1)
                    {
                        if (rpcTarget == networkRunner.LocalPlayer)
                            ExecuteLocal(entry, FinalOperationInfo);
                        else if (OnPlayerJoinedPatch.TryGetKey(rpcTarget, out var targetKey))
                        {
                            packet.ReliableKey = targetKey;
                            NetworkSender.SendToPlayer(rpcTarget, Serialize(packet));
                        }

                        return;
                    }


                    if (entry.Attr.Target == RPCTarget.Host)
                    {
                        ExecuteLocal(entry, FinalOperationInfo);
                    }
                    else if (entry.Attr.Target == RPCTarget.All)
                    {
                        if (ReliableSender != networkRunner.LocalPlayer)
                            ExecuteLocal(entry, FinalOperationInfo);
                    }
                    else if (entry.Attr.Target == RPCTarget.AllInclusive)
                    {
                        ExecuteLocal(entry, FinalOperationInfo);
                    }

                    foreach (var player in networkRunner.ActivePlayers.ToArray())
                    {
                        int id = player.PlayerId;

                        if (id == networkRunner.LocalPlayer)
                            continue;

                        if (entry.Attr.Target == RPCTarget.All && id == ReliableSender)
                            continue;

                        if (!OnPlayerJoinedPatch.TryGetKey(id, out var playerKey))
                            continue;

                        packet.ReliableKey = playerKey;

                        NetworkSender.SendToPlayer(id, Serialize(packet));
                    }
                }
                else
                {
                    int localId = Client.PState.PlayerId;

                    bool isKeyExchange = packet.Method == "RPC_ExchangeKeys";

                    bool hasKey = OnPlayerJoinedPatch.TryGetKey(localId, out var expectedKey);

                    if (!isKeyExchange)
                    {
                        if (hasKey && !string.IsNullOrEmpty(expectedKey))
                        {
                            if (packet.ReliableKey != expectedKey)
                                return;
                        }
                    }

                    int rpcTarget = -1;

                    var methodParams = entry.Method.GetParameters();
                    for (int i = 0; i < methodParams.Length && i < RpcArgs.Length; i++)
                    {
                        if (Attribute.IsDefined(methodParams[i], typeof(RPCTargetAttribute)))
                        {
                            if (RpcArgs[i] is int t)
                                rpcTarget = t;
                            break;
                        }
                    }

                    bool execute = false;

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

                        if (entry.Attr.Target == RPCTarget.Host)
                            execute = false;
                    }

                    if (execute)
                        ExecuteLocal(entry, FinalOperationInfo);
                }
            }
            catch { }
        }

        public static byte[] Serialize(RPCPacket packet)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(packet);
            byte[] SerializedPacket = new byte[json.Length + 1];
            SerializedPacket[0] = PacketConstants.MHAPI;
            Buffer.BlockCopy(json, 0, SerializedPacket, 1, json.Length);
            return SerializedPacket;
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