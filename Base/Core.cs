using Il2CppFusion;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections;
using MelonLoader;
using MessHallAPI.APIDebug;
using MessHallAPI.Config;
using MessHallAPI.Managers;
using MessHallAPI.Networking;
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
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                }
                catch { }
            }
            RPCRegistry.AutoDiscover();
            IsVR = Application.productName.Contains("VR");
        }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            SceneName = sceneName;
            InGame = SceneName != "Title" && SceneName != "Boot";
            if (SceneName == "Title")
            {
                ModStorage.LoadModStamp();
                RPCRegistry.ReliableKey = string.Empty;
            }
            if (InGame)
            {
                if (!ReferencesSet)
                {
                    ResetReferences();
                    MelonCoroutines.Start(DelayedReset());
                }
            }
        }

        private static System.Collections.IEnumerator DelayedReset()
        {
            if (networkRunner == null)
            {
                yield return new WaitForSeconds(2.5f);
                ResetReferences();
            }
        }


        public override void OnGUI()
        {
            float y = 10f;
            if (GUI.Button(new Rect(10, y, 100, 30), "RPC0"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 0, "hello");
            }
            y += 30f;
            if (GUI.Button(new Rect(10, y, 100, 30), "RPC1"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 1, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC2"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 2, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC3"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 3, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC4"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 4, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC5"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 5, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC6"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 6, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC7"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 7, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC8"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 8, "hello");
            }
            y += 30f;

            if (GUI.Button(new Rect(10, y, 100, 30), "RPC9"))
            {
                NetworkManager.InvokeRPC("MessHallAPI", "RPC_Sabotage", 9, "hello");
            }
        }
    }
}
