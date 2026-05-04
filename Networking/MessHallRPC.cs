namespace MessHallAPI.Networking
{
    /// <summary>
    /// Who is allowed to SEND this RPC.
    /// </summary>
    public enum RPCCaller
    {
        Anyone,
        HostOnly,
    }

    /// <summary>
    /// Who RECEIVES this RPC when sent. Use RPCTarget.All when using the [RPCTarget] attribute on a parameter to specify the target at runtime
    /// </summary>
    public enum RPCTarget
    {
        Host,
        All,
        AllInclusive,

        [Obsolete("InputAuthority is for debugging only and should not be used as a real RPCTarget. Use NetworkManager.ExecuteLocal")]
        InputAuthority
    }

    /// <summary>loc
    /// Marks a method as a MessHall networked RPC.
    ///
    /// Usage:
    ///   [MessHallRPC(RPCTarget.All, RPCCaller.Anyone)]
    ///   public void MyMethod(string arg) { ... }
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class MessHallRPCAttribute : Attribute
    {
        /// <summary>Who receives this RPC.</summary>
        public RPCTarget Target { get; }

        /// <summary>Who is allowed to send this RPC.</summary>
        public RPCCaller Caller { get; }

        /// <summary>
        /// Optional human-readable description shown in debug/logging.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        public MessHallRPCAttribute(RPCTarget target = RPCTarget.Host, RPCCaller caller = RPCCaller.Anyone)
        {
            Target = target;
            Caller = caller;
        }

    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RPCTargetAttribute : Attribute { }


    public struct MessHallRpcInfo
    {
        public int Sender;
        public bool IsHost;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RPCInfoAttribute : Attribute { }
}
