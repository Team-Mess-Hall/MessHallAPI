using MelonLoader;
using MessHallAPI.Networking;
using MessHallAPIExample.Managers;

namespace MessHallExample
{
  public class Core : MelonMod
  {
    public override void OnInitializeMelon()
    {
      object RpcMananger = new RPCManager();
      RPCRegistry.Register(RpcMananger, "MessHallExample");
    }
  }
}
