using Il2Cpp;
using Il2CppSG.Airlock.Sabotage;
using JetBrains.Annotations;
using MessHallAPI.Debugger;
using MessHallAPI.Networking;
using UnityEngine;


namespace Rewind.Managers
{
    public class RPCManager
    {
        public string ObjectId { get; private set; }


        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone, Description = "Test RPC for MessHallAPI")]
        public static void TestRPC1()
        {
            var sabo = UnityEngine.Object.FindObjectOfType<SabotageManager>();

            sabo.RPC_SendSabotageToAll(2, -1);
        }
    }
}