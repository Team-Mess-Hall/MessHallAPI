using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.XR;
using static UnityEngine.Object;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Debugger.Logging;
using Il2CppSG.Airlock.Graphics;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.Localization;

namespace MessHallAPI.Base
{
    public class References
    {
        public static XRRig Client;
        public static AirlockPeer Peer;
        public static GameStateManager GameState;
        public static SpawnManager Spawn;
        public static AirlockNetworkRunner networkRunner;
        public static PowerUpData powerData;
        public static NetworkedKillBehaviour Killing;
        public static RoleManager roleManager;
        public static LocalizationManager localization;

        public static void ResetReferences()
        {
            string trace = "";

            try
            {
                trace = "Setting Nulls";
                ReferencesSet = false;
                Client = null;
                Peer = null;
                GameState = null;
                Spawn = null;
                networkRunner = null;
                powerData = null;
                Killing = null;
                roleManager = null;

                trace = "Client";
                Client = FindObjectOfType<XRRig>();
                trace = "Peer";
                Peer = FindObjectOfType<AirlockPeer>();
                trace = "GameState";
                GameState = FindObjectOfType<GameStateManager>();
                trace = "Spawn";
                Spawn = FindObjectOfType<SpawnManager>();
                trace = "networkRunner";
                networkRunner = FindObjectOfType<AirlockNetworkRunner>();
                IsHost = Peer.Runner.LocalPlayer.PlayerId == 9;
                trace = "powerData";
                powerData = FindObjectOfType<PowerUpData>();
                trace = "NetworkedKillBehaviour";
                Killing = FindObjectOfType<NetworkedKillBehaviour>();
                trace = "RoleManager";
                roleManager = FindObjectOfType<RoleManager>();
                trace = "LocalizationManager";
                localization = FindObjectOfType<LocalizationManager>();
            
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
