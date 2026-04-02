using MelonLoader;
using MessHallAPIExample.Managers;

namespace MessHallExample
{
  public static class Core : MelonMod
  {
    public override void OnInitializeMelon
    {
      object RPCMananger = new RPCManager();
      RPCRegistry.Register(RPCManager, "MessHallExample");
    }
  }
}
