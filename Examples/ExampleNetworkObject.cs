using MessHallAPI.Networking;
using UnityEngine;

namespace MessHallAPIExample.Managers
{
    public class NetworkObjectManagerExample
    {
        ///<summary>
        ///Example object with MessHallNetworkTransform
        ///</summary>
        public static void test1()
        {
            GameObject objtest = new GameObject();
            objtest.AddComponent<MessHallNetworkTransform>();
            objtest.GetComponent<MessHallNetworkTransform>().Initialize(objtest.name);
        }
    }
}
