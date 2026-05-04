using Il2CppFusion;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections;
using Il2CppSystem.IO;
using MelonLoader;
using MessHallAPI.Config;
using MessHallAPI.Managers;
using MessHallAPI.Managers.Cosmetic;
using MessHallAPI.Networking;
using MessHallAPI.Patches;
using System.Text.Json;
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
                if (type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                }
            }
            RPCRegistry.AutoDiscover();
            IsVR = Application.productName.Contains("VR");
        }

        public override void OnUpdate()
        {
            CosmeticGUIManager.OnUpdate();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            SceneName = sceneName;
            InGame = SceneName != "Title" && SceneName != "Boot";
            if (SceneName == "Title")
            {
                ModStorage.LoadModStamp();
                RPCRegistry.ReliableKey = string.Empty;
                OnPlayerJoinedPatch.ReliableKeys.Clear();
                ReferencesSet = false;
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
            CosmeticGUIManager.OnGUI();
        }
    }
}
