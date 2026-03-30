using Il2Cpp;
using Il2CppSG.Airlock.Sabotage;
using JetBrains.Annotations;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;
using static UnityEngine.Object;


namespace MessHallAPIExample.Managers
{
    public class RPCManager
    {
        public string ObjectId { get; private set; }


        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone, Description = "Example RPC for MessHallAPI")]
        public static void RPC_SabotageTest()
        {
            var sabo = FindObjectOfType<SabotageManager>();

            sabo.RPC_SendSabotageToAll(2, -1);
        }



        [MessHallRPC(RPCTarget.All, RPCCaller.Anyone, Description = "Example RPC for MessHallAPI")]
        public static void RPC_AllLog(string msg)
        {
            Logging.Log($"Received RPC All:Anyone: {msg}");
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.Anyone, Description = "Example RPC for MessHallAPI")]
        public static void RPC_AllIncLog(string msg)
        {
            Logging.Log($"Received RPC AllInc:Anyone: {msg}");
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.HostOnly, Description = "Example RPC for MessHallAPI")]
        public static void RPC_HostCallLog(string msg)
        {
            Logging.Log($"Received RPC AllInc:HostCall: {msg}");
        }


        [MessHallRPC(RPCTarget.All, RPCCaller.Anyone, Description = "Example RPC for MessHallAPI")]
        public static void RPC_targetLog([RPCTarget] int target, string msg)
        {
            Logging.Log($"Received targeted RPC: {msg}");
        }



        public static void InvokeExample1()
        {
            NetworkManager.InvokeRPC("MessHallAPI", "RPC_SabotageTest");
        }
        ///<summary>
        /// NetworkManager.InvokeRPC(string ModName, string MethodName) these are optional>>(int SpecificPlayerID, string Message)
        ///</summary>
    }
}
