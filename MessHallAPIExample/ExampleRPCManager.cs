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

        public static void InvokeExample1()
        {
            NetworkManager.InvokeRPC("MessHallAPI", "RPC_SabotageTest");
        }
        ///<summary>
        /// NetworkManager.InvokeRPC(string ModName, string MethodName) these are optional>>(int SpecificPlayerID, string Message)
        ///</summary>
    }
}
