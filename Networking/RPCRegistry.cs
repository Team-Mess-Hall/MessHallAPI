using System.Reflection;
using MelonLoader;
using MessHallAPI.Debugger;

namespace MessHallAPI.Networking
{
    public static class RPCRegistry
    {
        public class RPCPacket
        {
            public string? ModId { get; set; }
            public string? ReliableKey { get; set; }
            public string? Method { get; set; }
            public int ActorId { get; set; }
            public object[]? Args { get; set; }
        }

        internal record RPCEntry(
            object Owner,
            MethodInfo Method,
            MessHallRPCAttribute Attr,
            string ModId
        );

        private static readonly Dictionary<string, RPCEntry> _entries = new();
        internal static string ReliableKey = "";
        public static void Register(object instance, string modId)
        {
            ScanAndRegister(instance, instance.GetType(), modId);
        }

        internal static void AutoDiscover()
        {
            foreach (var mod in MelonMod.RegisteredMelons)
            {
                var assembly = mod.GetType().Assembly;
                var modId = mod.Info.Name;

                foreach (var type in assembly.GetTypes())
                {
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<MessHallRPCAttribute>();
                        if (attr == null)
                            continue;

                        object instance = null;

                        if (!method.IsStatic)
                        {
                            instance = TryGetInstance(type);
                            if (instance == null)
                            {
                                try { instance = Activator.CreateInstance(type); }
                                catch { }
                            }
                        }

                        string key = $"{modId}::{method.Name}";

                        if (_entries.ContainsKey(key))
                        {
                            Logging.Warn($"Duplicate RPC '{key}' — skipping.");
                            continue;
                        }

                        ValidateSignature(method);

                        _entries[key] = new RPCEntry(instance, method, attr, modId);

                        Logging.Log($"[RPC REGISTERED] {key}");
                    }
                }
            }

            Logging.Log($"[RPC] Total Registered: {_entries.Count}");
        }

        public static void Unregister(string modId)
        {
            var keys = _entries
                .Where(kv => kv.Value.ModId == modId)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in keys)
                _entries.Remove(key);

            Logging.Warn($"Unregistered {keys.Count} RPCs for mod '{modId}'");
        }

        internal static bool TryGet(string key, out RPCEntry entry)
            => _entries.TryGetValue(key, out entry);

        internal static IReadOnlyDictionary<string, RPCEntry> All => _entries;

        private static void ScanAndRegister(object instance, Type type, string modId)
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<MessHallRPCAttribute>();
                if (attr == null)
                    continue;

                object owner = method.IsStatic ? null : instance;

                if (!method.IsStatic && owner == null)
                {
                    try { owner = Activator.CreateInstance(type); }
                    catch { }
                }

                string key = $"{modId}::{method.Name}";

                if (_entries.ContainsKey(key))
                {
                    Logging.Warn($"Duplicate RPC '{key}' — skipping.");
                    continue;
                }

                ValidateSignature(method);

                _entries[key] = new RPCEntry(owner, method, attr, modId);

                Logging.Log($"[RPC REGISTERED] {key}");
            }
        }

        private static void ValidateSignature(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException($"RPC '{method.Name}' must return void.");

            var supported = new[] {
                typeof(int), typeof(float), typeof(bool),
                typeof(string), typeof(byte)
            };

            foreach (var param in method.GetParameters())
            {
                if (Attribute.IsDefined(param, typeof(RPCInfoAttribute)))
                {
                    if (param.ParameterType != typeof(MessHallRpcInfo))
                    {
                        throw new NotSupportedException($"RPC '{method.Name}' has invalid RPCInfo parameter type '{param.ParameterType.Name}'");
                    }

                    continue;
                }

                Type t = param.ParameterType;

                if (t != typeof(int) &&
                    t != typeof(float) &&
                    t != typeof(bool) &&
                    t != typeof(string) &&
                    t != typeof(byte))
                {
                    throw new NotSupportedException(
                        $"RPC '{method.Name}' has unsupported parameter type '{t.Name}'. Supported: int, float, bool, string, byte.");
                }
            }
        }

        private static object TryGetInstance(Type type)
        {
            var instanceProp = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
                            ?? type.GetProperty("Singleton", BindingFlags.Static | BindingFlags.Public);

            if (instanceProp != null)
                return instanceProp.GetValue(null);

            return null;
        }

        public static void Execute(string modId, string methodName, params object[] args)
        {
            string key = $"{modId}::{methodName}";

            if (!_entries.TryGetValue(key, out var entry))
            {
                Logging.Error($"RPC not found: {key}");
                foreach (var k in _entries.Keys)
                    Logging.Log($"RPC: {k}");
                return;
            }

            try
            {
                entry.Method.Invoke(entry.Method.IsStatic ? null : entry.Owner, args);
            }
            catch (Exception ex)
            {
                Logging.Error($"RPC invoke failed: {ex}");
            }
        }
    }
}