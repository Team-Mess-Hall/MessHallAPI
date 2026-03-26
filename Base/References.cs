using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.XR;
using static UnityEngine.Object;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Debugger.Logging;

namespace MessHallAPI.Base
{
    public class References
    {
        public static XRRig Client;
        public static AirlockPeer Peer;
        public static GameStateManager GameState;
        public static SpawnManager Spawn;

        public static AirlockNetworkRunner networkRunner;
        public static void ResetReferences()
        {
            string trace = "";

            try
            {
                trace = "Setting Nulls";
                ReferencesSet = false;
                Client = null;

                trace = "Client";
                Client = FindObjectOfType<XRRig>();
                trace = "GameState";
                GameState = FindObjectOfType<GameStateManager>();
                trace = "Spawn";
                Spawn = FindObjectOfType<SpawnManager>();
                trace = "networkRunner";
                networkRunner = FindObjectOfType<AirlockNetworkRunner>();
                IsHost = networkRunner.LocalPlayer.PlayerId == 9;

            }
            catch
            {
                Error("Error occured when getting reference: " + trace);
            }
            finally
            {
                ReferencesSet = true;
                Log("All references found!");
            }
        }
    }
}
