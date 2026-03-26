using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using MessHallAPI.Config;
using MessHallAPI.Managers;
using MessHallAPI.Networking;
using MessHallAPI.Patches;
using Rewind.Managers;
using UnityEngine;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Base
{
    public class Core : MelonMod
    {
        public static string SceneName;
        public static bool ShouldMakeAnotherInstance()
        {
            return System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length == 1;
        }
        public override void OnInitializeMelon()
        {
            foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                try { ClassInjector.RegisterTypeInIl2Cpp(type); }
                catch { }
            }

            if (InstanceConfig.MultipleInstancesEnabled && ShouldMakeAnotherInstance())
            {
                for (int i = 0; i < InstanceConfig.InstanceAmount; i++)
                {
                    System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                }
            }

            IsVR = Application.productName.Contains("VR");
            RPCRegistry.AutoDiscover();


        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            SceneName = sceneName;
            InGame = SceneName != "Title" && SceneName != "Boot";
            if (SceneName == "Title")
            {
                ModStorage.LoadModStamp();
            }
            if (InGame)
            {
                if (ReferencesSet != true)
                {
                    ResetReferences();
                }
            }
        }

        private Rect window = new Rect(10, 10, 220, 140);

        public override void OnGUI()
        {
            GUI.depth = -1000;
            window = GUI.Window(1337, window, (GUI.WindowFunction)DrawWindow, "RPC Test");
        }

        private void DrawWindow(int id)
        {
            if (GUI.Button(new Rect(10, 20, 200, 30), "SendRpc"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "TestRPC1");
            }

            if (GUI.Button(new Rect(10, 50, 200, 30), "Spoof GUID"))
            {
                RPCRegistry.ReliableKey = "fuckyou";
            }


            if (GUI.Button(new Rect(10, 70, 200, 30), "Fix GUID"))
            {
                RPCRegistry.ReliableKey = OnPlayerJoinedPatch.Keys[References.networkRunner.LocalPlayer];
                MelonLogger.Msg($"Set RPC key to {RPCRegistry.ReliableKey}");
            }
            GUI.DragWindow();
        }
    }
}
