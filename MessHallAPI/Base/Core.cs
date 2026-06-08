using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MessHallAPI.Managers;
using MessHallAPI.Managers.ActionSystem;
using MessHallAPI.Managers.Cosmetic;
using MessHallAPI.Managers.Role;
using MessHallAPI.Managers.RoleSettings;
using MessHallAPI.Networking;
using MessHallAPI.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Base
{
    [BepInPlugin("plugin.teammesshall.com", "MessHallAPI", "1.0.0")]
    public class Core : BasePlugin
    {
        public static string SceneName;

        public override void Load()
        {
            foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                }
            }

            RPCRegistry.AutoDiscover();
            PowerRegistration.AutoRegister();
            CustomRoleManager.AutoRegisterRoles();
            TargetedActionRegistration.AutoRegister();
            Custom3DPanelManager.AutoRegisterPanels();
            VanillaRoleManager.AutoRegisterRoles();
            IsVR = Application.productName.Contains("VR");

            var go = new GameObject("MessHallAPI_Runner");
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<CoreBehaviour>();
            new Harmony("plugin.teammesshall.com").PatchAll();
        }
    }

    public class CoreBehaviour : MonoBehaviour
    {
        public CoreBehaviour(IntPtr ptr) : base(ptr) { }
        private Action<Scene, LoadSceneMode> _onSceneLoaded;
        public static CoreBehaviour Instance { get; private set; }

        private void Start()
        {
            Instance = this;
            _onSceneLoaded = new Action<Scene, LoadSceneMode>(OnSceneLoaded);
            SceneManager.sceneLoaded += _onSceneLoaded;
        }

        private void Update()
        {
            PowerRegistration.OnUpdate();
            CosmeticGUIManager.OnUpdate();
            SettingsManager.OnUpdate();
            Custom3DPanelManager.OnUpdate();
            CustomButtonSystem.OnUpdate();
            TargetedActionRegistration.OnUpdate();
            ButtonPositionManager.OnUpdate();

            if (InGame && RoleSelectionPanel.kbm != null &&
                RoleSelectionPanel.kbm.XTile != 5 &&
                RoleSelectionPanel.kbm.XTile != 0)
            {
                RoleSelectionPanel.kbm.SetTileOffset(
                    KeybindManager.StringToV2(KeybindManager.fKey));
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Core.SceneName = scene.name;
            InGame = Core.SceneName != "Title" && Core.SceneName != "Boot";

            if (Core.SceneName == "Title")
            {
                ModStorage.LoadModStamp();
                RPCRegistry.ReliableKey = string.Empty;
                OnPlayerJoinedPatch.ReliableKeys.Clear();
                NameplateRegistry._registry.Clear();
                ReferencesSet = false;
            }

            if (InGame)
            {
                if (!ReferencesSet)
                {
                    ResetReferences();
                    StartCoroutine(DelayedReset().ToString());
                }
                ModStorage.LoadIcons();
            }
        }

        private static System.Collections.IEnumerator DelayedReset()
        {
            yield return new WaitForSeconds(2.5f);
            if (networkRunner == null)
                ResetReferences();
        }

        private void OnGUI()
        {
            CosmeticGUIManager.OnGUI();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= _onSceneLoaded;
        }
    }
}