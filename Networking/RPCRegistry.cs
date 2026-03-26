using System.Reflection;
using MelonLoader;
using MessHallAPI.Debugger;

namespace MessHallAPI.Networking
{
    public static class RPCRegistry
    {
        internal record RPCEntry(
            object Owner,
            MethodInfo Method,
            MessHallRPCAttribute Attr,
            string ModId
        );

        private static readonly Dictionary<string, RPCEntry> _entries = new();
        internal static string ReliableKey = string.Empty;

        public static void Register(object instance, string modId)
        {
            ScanAndRegister(instance, instance.GetType(), modId);
        }

        internal static void AutoDiscover()
        {
            foreach (MelonMod melon in MelonMod.RegisteredMelons)
            {
                var assembly = melon.GetType().Assembly;
                var modId = melon.Info.Name;

                foreach (var type in assembly.GetTypes())
                {
                    MethodInfo[]? methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (MethodInfo method in methods)
                    {
                        var attr = method.GetCustomAttribute<MessHallRPCAttribute>();
                        if (attr == null)
                            continue;

                        var owner = method.IsStatic ? null : TryGetInstance(type);
                        if (!method.IsStatic && owner == null)
                        {
                            try { owner = Activator.CreateInstance(type); } catch { }
                        }

                        var key = $"{modId}::{method.Name}";
                        if (_entries.ContainsKey(key))
                            continue;

                        ValidateSignature(method);

                        _entries[key] = new RPCEntry(owner, method, attr, modId);
                        Logging.Log($"RPC {key}");
                    }
                }
            }
        }

        public static void Unregister(string modId)
        {
            var keys = _entries.Where(x => x.Value.ModId == modId).Select(x => x.Key).ToList();
            foreach (var k in keys)
                _entries.Remove(k);
        }

        internal static bool TryGet(string key, out RPCEntry entry)
            => _entries.TryGetValue(key, out entry);

        internal static IReadOnlyDictionary<string, RPCEntry> All => _entries;

        private static void ScanAndRegister(object instance, Type type, string modId)
        {
            foreach (MethodInfo? method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var RpcAttribute = method.GetCustomAttribute<MessHallRPCAttribute>();
                if (RpcAttribute == null)
                    continue;

                Object? owner = method.IsStatic ? null : instance;

                var key = $"{modId}::{method.Name}";
                if (_entries.ContainsKey(key))
                    continue;

                ValidateSignature(method);

                _entries[key] = new RPCEntry(owner, method, RpcAttribute, modId);
                Logging.Log($"RPC {key}");
            }
        }

        private static void ValidateSignature(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException();

            Type[] type = new[] { typeof(int), typeof(float), typeof(bool), typeof(string), typeof(byte) };

            foreach (ParameterInfo? property in method.GetParameters())
                if (!type.Contains(property.ParameterType))
                    throw new NotSupportedException();
        }

        private static object? TryGetInstance(Type type)
        {
            PropertyInfo? property = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public) ?? type.GetProperty("Singleton", BindingFlags.Static | BindingFlags.Public);

            if (property != null) return property.GetValue(null);
            return null;
        }
    }
}