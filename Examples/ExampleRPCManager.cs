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

        public static void InvokeExample1
        {
            NetworkManager.InvokeRPC("MessHallAPIExample", "RPC_SabotageTest", );
        }
        ///<summary>
        /// NetworkManager.InvokeRPC(string ModName, string MethodName) these are optional>>(int SpecificPlayerID, string Message)
        ///</summary>
    }
}
