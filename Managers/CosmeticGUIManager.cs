using Il2CppSG.Airlock;
using MessHallAPI.Base;
using MessHallAPI.Debugger;
using MessHallAPI.Managers.Cosmetic;
using MessHallAPI.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using static MelonLoader.MelonLogger;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;
using static UnityEngine.Object;

namespace MessHallAPI.Managers
{
    internal class CosmeticGUIManager
    {
        private static bool _visible = false;
        private static Vector2 _scrollPos = Vector2.zero;
        private const int WindowId = 9001;
        private static int _activeTab = 0;

        private static readonly string[] _row1Tabs = { "Nameplates", "Pets", "Hats", "Visors", "Backpacks" };
        
        // nameplates
        private static Sprite? Infectionplate = null;
        private static Sprite? LightsOutplate = null;
        private static Sprite? ContainmentPlate = null;
        private static Sprite? BoneBashPlate = null;
        private static Sprite? RoundUpPlate = null;

        public static void OnUpdate()
        {
            if (InGame && GameState.InLobbyState())
            {
                if (Keyboard.current.rightAltKey.wasPressedThisFrame)
                    _visible = !_visible;
                
                CustomPetManager.RegisterPet("MessHallAPI", "No Pet", null);

                CustomBackpackManager.RegisterBackpack("MessHallAPI", "No Backpack", null);

                CustomHatManager.RegisterHat("MessHallAPI", "No Hat", null);

                CustomVisorManager.RegisterVisor("MessHallAPI", "No Visor", null);

                if (Infectionplate == null)
                {
                    Infectionplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.infectionlogo.png");
                    LightsOutplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.LightOut.png");
                    ContainmentPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.Containment.png");
                    BoneBashPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.BoneBash.png");
                    RoundUpPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.Roundup.png");
                }

                if (Infectionplate != null && NameplateRegistry.GetAll().Count == 0)
                {
                    NameplateRegistry.Register("MessHallAPI", "InfectionPlate", Infectionplate);
                    NameplateRegistry.Register("MessHallAPI", "LightsOutPlate", LightsOutplate);
                    NameplateRegistry.Register("MessHallAPI", "ContainmentPlate", ContainmentPlate);
                    NameplateRegistry.Register("MessHallAPI", "BoneBashPlate", BoneBashPlate);
                    NameplateRegistry.Register("MessHallAPI", "RoundUpPlate", RoundUpPlate);
                }
            }
            else
            {
                _visible = false;
            }
        }


        public static void OnGUI()
        {
            if (InGame && GameState.InLobbyState())
            {
                if (!_visible) return;
                GUI.Window(WindowId, new Rect(100, 100, 400, 580), (GUI.WindowFunction)DrawWindow, "Cosmetics");
            }
            else
            {
                _visible = false;
            }
        }

        private static void DrawWindow(int id)
        {
            _activeTab = GUI.Toolbar(new Rect(10, 25, 380, 30), _activeTab, _row1Tabs);

            switch (_activeTab)
            {
                case 0: DrawNameplatesTab(); break;
                case 1: DrawPetsTab(); break;
                case 2: DrawHatsTab(); break;
                case 3: DrawVisorsTab(); break;
                case 4: DrawBackpacksTab(); break;
            }

            GUI.DragWindow();
        }

        private static void DrawNameplatesTab()
        {
            var nameplates = NameplateRegistry.GetAll().ToList();
            if (nameplates.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No nameplates registered."); // because we register a Plates by default, if this shows something broke.
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, nameplates.Count * 80)
            );

            for (int i = 0; i < nameplates.Count; i++)
            {
                string capturedMod = nameplates[i].Key.Item1;
                string capturedId = nameplates[i].Key.Item2;
                Sprite sprite = nameplates[i].Value;

                Rect btnRect = new Rect(0, i * 80, 360, 70);
                if (sprite != null)
                {
                    if (GUI.Button(btnRect, new GUIContent(capturedId, sprite.texture, "")))
                    {
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetNameplate", Client.PState.PlayerId, capturedMod, capturedId);
                        _visible = false;
                    }
                }
                else
                {
                    if (GUI.Button(btnRect, capturedId))
                    {
                        Logging.Warn($"Sprite for {capturedMod}:{capturedId} is null. Cannot set nameplate.");
                        _visible = false;
                    }
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawPetsTab()
        {
            var pets = CustomPetManager.GetAllRegistered().ToList();
            if (pets.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No pets registered."); // because we register a "No Pet" option by default, if this shows something broke.
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, pets.Count * 60)
            );

            for (int i = 0; i < pets.Count; i++)
            {
                string capturedMod = pets[i].Key.Item1;
                string capturedId = pets[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomPetManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearPet", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetPet", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawHatsTab()
        {
            var hats = CustomHatManager.GetAllRegistered().ToList();
            if (hats.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No hats registered."); // because we register a "No Hat" option by default, if this shows something broke.
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, hats.Count * 60)
            );

            for (int i = 0; i < hats.Count; i++)
            {
                string capturedMod = hats[i].Key.Item1;
                string capturedId = hats[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomHatManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearHat", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetHat", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawVisorsTab()
        {
            var visors = CustomVisorManager.GetAllRegistered().ToList();
            if (visors.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No visors registered."); // because we register a "No Visor" option by default, if this shows something broke.
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, visors.Count * 60)
            );

            for (int i = 0; i < visors.Count; i++)
            {
                string capturedMod = visors[i].Key.Item1;
                string capturedId = visors[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomVisorManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearVisor", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetVisor", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawBackpacksTab()
        {
            var backpacks = CustomBackpackManager.GetAllRegistered().ToList();
            if (backpacks.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No backpacks registered.");
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, backpacks.Count * 60)
            );

            for (int i = 0; i < backpacks.Count; i++)
            {
                string capturedMod = backpacks[i].Key.Item1;
                string capturedId = backpacks[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomBackpackManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearBackpack", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetBackpack", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }

        /*
        private static void DrawSkinsTab()
        {
            var skins = CustomSkinManager.GetAllRegistered().ToList();
            if (skins.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No skins registered.");
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, skins.Count * 60)
            );

            for (int i = 0; i < skins.Count; i++)
            {
                string capturedMod = skins[i].Key.Item1;
                string capturedId = skins[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomSkinManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearSkin", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetSkin", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawGlovesTab()
        {
            var gloves = CustomGloveManager.GetAllRegistered().ToList();
            if (gloves.Count == 0)
            {
                GUI.Label(new Rect(10, 65, 380, 30), "No gloves registered.");
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 65, 380, 505),
                _scrollPos,
                new Rect(0, 0, 360, gloves.Count * 60)
            );

            for (int i = 0; i < gloves.Count; i++)
            {
                string capturedMod = gloves[i].Key.Item1;
                string capturedId = gloves[i].Key.Item2;

                Rect btnRect = new Rect(0, i * 60, 360, 50);
                if (GUI.Button(btnRect, $"{capturedMod}: {capturedId}"))
                {
                    if (CustomGloveManager.GetRegisteredPrefab(capturedMod, capturedId) == null)
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_ClearGlove", Client.PState.PlayerId);
                    else
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetGlove", Client.PState.PlayerId, capturedMod, capturedId);
                    _visible = false;
                }
            }

            GUI.EndScrollView();
        }
        */
    }
}