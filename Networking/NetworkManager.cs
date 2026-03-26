using System.Runtime.CompilerServices;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Sabotage;
using MelonLoader;
using MessHallAPI.Base;
using MessHallAPI.Config;
using MessHallAPI.Debugger;
using MessHallAPI.Patches;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Networking
{
    public static class NetworkManager
    {

        public static void InvokeRPC(string ModId, string Method, params object[] Args)
        {
            if (!Settings.InGame)
            {
                MelonLogger.Msg("[RPC] Not in game");
                return;
            }

            string Key = ModId + "::" + Method;

            if (!RPCRegistry.TryGet(Key, out var Entry))
            {
                MelonLogger.Msg($"[RPC] Not registered: {Key}");
                return;
            }

            if (Entry.Attr.Caller == RPCCaller.HostOnly && !Settings.IsHost)
            {
                MelonLogger.Msg($"[RPC] Blocked HostOnly: {Key}");
                return;
            }

            var Encoding = System.Text.Encoding.UTF8;
            string ReliableKey = RPCRegistry.ReliableKey ?? "";
            int Actor = References.networkRunner.LocalPlayer.PlayerId;

            byte[] Buffer = new byte[1024];
            int Offset = 0;

            Buffer[Offset++] = PacketConstants.MHAPI;

            void WriteString(string Str)
            {
                var Bytes = Encoding.GetBytes(Str ?? "");
                Buffer[Offset++] = (byte)TypeTag.String;
                System.Buffer.BlockCopy(BitConverter.GetBytes(Bytes.Length), 0, Buffer, Offset, 4);
                Offset += 4;
                System.Buffer.BlockCopy(Bytes, 0, Buffer, Offset, Bytes.Length);
                Offset += Bytes.Length;
            }

            void WriteInt(int Val)
            {
                Buffer[Offset++] = (byte)TypeTag.Int;
                System.Buffer.BlockCopy(BitConverter.GetBytes(Val), 0, Buffer, Offset, 4);
                Offset += 4;
            }

            WriteString(ModId);
            WriteInt(Actor);
            WriteString(ReliableKey);
            WriteString(Method);

            System.Buffer.BlockCopy(BitConverter.GetBytes(Args.Length), 0, Buffer, Offset, 4);
            Offset += 4;

            foreach (var Arg in Args)
            {
                switch (Arg)
                {
                    case int I:
                        WriteInt(I);
                        break;

                    case float F:
                        Buffer[Offset++] = (byte)TypeTag.Float;
                        System.Buffer.BlockCopy(BitConverter.GetBytes(F), 0, Buffer, Offset, 4);
                        Offset += 4;
                        break;

                    case bool B:
                        Buffer[Offset++] = (byte)TypeTag.Bool;
                        Buffer[Offset++] = B ? (byte)1 : (byte)0;
                        break;

                    case byte BT:
                        Buffer[Offset++] = (byte)TypeTag.Byte;
                        Buffer[Offset++] = BT;
                        break;

                    case string S:
                        WriteString(S);
                        break;

                    default:
                        MelonLogger.Msg($"[RPC] Unknown arg type: {Arg?.GetType()}");
                        break;
                }
            }

            byte[] Final = new byte[Offset];
            System.Buffer.BlockCopy(Buffer, 0, Final, 0, Offset);

            MelonLogger.Msg($"[RPC] {Key} | Actor {Actor} | Args {Args.Length} | lentgh {Final.Length}");

            switch (Entry.Attr.Target)
            {
                case RPCTarget.Host:
                    MelonLogger.Msg("[RPC] sent to host i think");

                    if (Settings.IsHost)
                    {
                        MelonLogger.Msg("[RPC] Executed locally prob");
                        ExecuteLocal(Entry, Args);
                    }
                    else
                    {
                        NetworkSender.SendToServer(Final);
                    }
                    break;

                case RPCTarget.All:
                    MelonLogger.Msg("[RPC SEND] All");
                    NetworkSender.SendToAll(Final, false);
                    break;

                case RPCTarget.AllInclusive:
                    MelonLogger.Msg("[RPC SEND] AllInclusive");
                    ExecuteLocal(Entry, Args);
                    NetworkSender.SendToAll(Final, false);
                    break;
            }
        }

        public static void OperationReceived(PlayerRef Sender, Il2CppStructArray<byte> DataArray)
        {
            if (!Settings.InGame)
                return;

            byte[] Data = DataArray;

            if (Data.Length < 2 || Data[0] != PacketConstants.MHAPI)
                return;

            var Encoding = System.Text.Encoding.UTF8;
            int Offset = 1;

            string ReadString()
            {
                int Len = BitConverter.ToInt32(Data, Offset);
                Offset += 4;
                string Str = Encoding.GetString(Data, Offset, Len);
                Offset += Len;
                return Str;
            }

            int ReadInt()
            {
                int Val = BitConverter.ToInt32(Data, Offset);
                Offset += 4;
                return Val;
            }

            try
            {
                Offset++;
                string ModId = ReadString();

                Offset++;
                int Actor = ReadInt();

                Offset++;
                string Key = ReadString();

                Offset++;
                string Method = ReadString();

                int Count = ReadInt();

                MelonLogger.Msg($"[RPC recv] {ModId}::{Method} | From {Actor} | Args {Count}");

                if (!string.IsNullOrEmpty(Key))
                {
                    OnPlayerJoinedPatch.Keys[Actor] = Key;
                    RPCRegistry.ReliableKey = Key;
                }

                string RpcKey = ModId + "::" + Method;

                if (!RPCRegistry.TryGet(RpcKey, out var Entry))
                {
                    MelonLogger.Msg($"[RPC recv] Not registered: {RpcKey}");
                    return;
                }

                if (Entry.Attr.Caller == RPCCaller.HostOnly && Sender.PlayerId != 9)
                {
                    MelonLogger.Msg($"[RPC recv] Blocked HostOnly: {RpcKey}");
                    return;
                }

                object[] Args = new object[Count];

                for (int i = 0; i < Count; i++)
                {
                    var Type = (TypeTag)Data[Offset++];

                    switch (Type)
                    {
                        case TypeTag.Int:
                            Args[i] = ReadInt();
                            break;

                        case TypeTag.Float:
                            Args[i] = BitConverter.ToSingle(Data, Offset);
                            Offset += 4;
                            break;

                        case TypeTag.Bool:
                            Args[i] = Data[Offset++] == 1;
                            break;

                        case TypeTag.Byte:
                            Args[i] = Data[Offset++];
                            break;

                        case TypeTag.String:
                            Args[i] = ReadString();
                            break;
                    }
                }

                MelonLogger.Msg($"[RPC OK] {RpcKey}");

                ExecuteLocal(Entry, Args);
            }
            catch (Exception Ex)
            {
                MelonLogger.Error($"[RPC ERROR] {Ex}");
            }
        }














        //  No registered RPC: 'MessHallAPITest::SabotageRPC'






        private static byte[] Serialize(string modId, string methodName, object[] args, string reliableKey, int actor)
        {
            var enc = System.Text.Encoding.UTF8;

            // Pre-encode strings (avoid double work)
            var modBytes = enc.GetBytes(modId ?? "");
            var keyBytes = enc.GetBytes(reliableKey ?? "");
            var methodBytes = enc.GetBytes(methodName ?? "");

            int size =
                1 + // MHAPI
                4 + modBytes.Length +
                4 + // actor
                4 + keyBytes.Length +
                4 + methodBytes.Length +
                4; // arg count

            // estimate args
            foreach (var arg in args)
            {
                size += 1;
                switch (arg)
                {
                    case int: size += 4; break;
                    case float: size += 4; break;
                    case bool: size += 1; break;
                    case byte: size += 1; break;
                    case string s:
                        var b = enc.GetBytes(s);
                        size += 4 + b.Length;
                        break;
                }
            }

            byte[] buffer = new byte[size];
            int offset = 0;

            buffer[offset++] = PacketConstants.MHAPI;

            Buffer.BlockCopy(BitConverter.GetBytes(modBytes.Length), 0, buffer, offset, 4); offset += 4;
            Buffer.BlockCopy(modBytes, 0, buffer, offset, modBytes.Length); offset += modBytes.Length;

            Buffer.BlockCopy(BitConverter.GetBytes(actor), 0, buffer, offset, 4); offset += 4;

            Buffer.BlockCopy(BitConverter.GetBytes(keyBytes.Length), 0, buffer, offset, 4); offset += 4;
            Buffer.BlockCopy(keyBytes, 0, buffer, offset, keyBytes.Length); offset += keyBytes.Length;

            Buffer.BlockCopy(BitConverter.GetBytes(methodBytes.Length), 0, buffer, offset, 4); offset += 4;
            Buffer.BlockCopy(methodBytes, 0, buffer, offset, methodBytes.Length); offset += methodBytes.Length;

            Buffer.BlockCopy(BitConverter.GetBytes(args.Length), 0, buffer, offset, 4); offset += 4;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case int i:
                        buffer[offset++] = (byte)TypeTag.Int;
                        Buffer.BlockCopy(BitConverter.GetBytes(i), 0, buffer, offset, 4);
                        offset += 4;
                        break;

                    case float f:
                        buffer[offset++] = (byte)TypeTag.Float;
                        Buffer.BlockCopy(BitConverter.GetBytes(f), 0, buffer, offset, 4);
                        offset += 4;
                        break;

                    case bool b:
                        buffer[offset++] = (byte)TypeTag.Bool;
                        buffer[offset++] = b ? (byte)1 : (byte)0;
                        break;

                    case byte bt:
                        buffer[offset++] = (byte)TypeTag.Byte;
                        buffer[offset++] = bt;
                        break;

                    case string s:
                        var sb = enc.GetBytes(s ?? "");
                        buffer[offset++] = (byte)TypeTag.String;
                        Buffer.BlockCopy(BitConverter.GetBytes(sb.Length), 0, buffer, offset, 4); offset += 4;
                        Buffer.BlockCopy(sb, 0, buffer, offset, sb.Length); offset += sb.Length;
                        break;
                }
            }

            return buffer;
        }

        private static void Deserialize(byte[] data, out string modId, out string methodName, out object[] args)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            modId = reader.ReadString();
            methodName = reader.ReadString();
            int count = reader.ReadInt32();
            args = new object[count];

            for (int i = 0; i < count; i++)
            {
                args[i] = (TypeTag)reader.ReadByte() switch
                {
                    TypeTag.Int => reader.ReadInt32(),
                    TypeTag.Float => reader.ReadSingle(),
                    TypeTag.Bool => reader.ReadBoolean(),
                    TypeTag.String => reader.ReadString(),
                    TypeTag.Byte => reader.ReadByte(),
                    var t => throw new NotSupportedException($"[MessHallAPI] Unknown TypeTag: {t}")
                };
            }
        }

        private enum TypeTag : byte { Int, Float, Bool, String, Byte }

        private static void ExecuteLocal(RPCRegistry.RPCEntry entry, object[] args)
        {
            try
            {
                entry.Method.Invoke(entry.Owner, args);
            }
            catch (Exception ex)
            {
                Logging.Error($"RPC execution error in '{entry.Method.Name}': " + $"{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private static bool ValidateKey(byte[] payload, out PlayerRef ReliableSender)
        {
            ReliableSender = default;

            try
            {
                using var ms = new MemoryStream(payload);
                using var reader = new BinaryReader(ms);

                string key = reader.ReadString();
                int actor = reader.ReadInt32();

                if (!OnPlayerJoinedPatch.Keys.TryGetValue(actor, out var expectedKey) || expectedKey != key)
                    return false;

                ReliableSender = actor;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }



    internal static class PacketConstants
    {
        public const byte MHAPI = 0x4d;
    }
}