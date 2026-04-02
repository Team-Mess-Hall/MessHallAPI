using MelonLoader;

namespace MessHallExample
{
  public static class Core : MelonMod
  {
    public override void OnInitializeMelon
    {
      object RPCMananger = new ExampleRPCManager();
      RPCRegistry.Register(RPCManager, "MessHallExample");
    }
  }
}
