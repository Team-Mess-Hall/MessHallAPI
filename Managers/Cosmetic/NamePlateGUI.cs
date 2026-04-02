using static MessHallAPI.Base.References;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Config.Settings;
using MessHallAPI.Base;
using MessHallAPI.Networking;

namespace MessHallAPI.Managers.Cosmetic
{
    public static class NameplateGUI
    {
        private static bool _visible = false;
        private static Vector2 _scrollPos = Vector2.zero;
        private const int WindowId = 9001;
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
                {
                    _visible = !_visible; 
                }
                if (Infectionplate == null)
                {
                    Infectionplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.infectionlogo.png");
                    LightsOutplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.LightOut.png");
                    ContainmentPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.Containment.png");
                    BoneBashPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.BoneBash.png");
                    RoundUpPlate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.Roundup.png");
                }
                if (Infectionplate != null)
                {
                    if (NameplateRegistry.GetAll().Count == 0)
                    {
                        NameplateRegistry.Register("MessHallAPI", "InfectionPlate", Infectionplate);
                        NameplateRegistry.Register("MessHallAPI", "LightsOutPlate", LightsOutplate);
                        NameplateRegistry.Register("MessHallAPI", "ContainmentPlate", ContainmentPlate);
                        NameplateRegistry.Register("MessHallAPI", "BoneBashPlate", BoneBashPlate);
                        NameplateRegistry.Register("MessHallAPI", "RoundUpPlate", RoundUpPlate);
                    }
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

                GUI.Window(WindowId, new Rect(100, 100, 400, 500), (GUI.WindowFunction)DrawWindow, "Nameplates");
            }
            else
            {
                _visible = false;
            }
        }

        private static void DrawWindow(int id)
        {
            var nameplates = NameplateRegistry.GetAll().ToList();
            if (nameplates.Count == 0)
            {
                GUI.Label(new Rect(10, 30, 380, 30), "No nameplates registered.");
                return;
            }

            _scrollPos = GUI.BeginScrollView(
                new Rect(10, 30, 380, 450),
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
                    GUIContent content = new GUIContent(capturedId, sprite.texture, "");
                    if (GUI.Button(btnRect, content))
                    {
                        NetworkManager.InvokeRPC("MessHallAPI", "RPC_SetNameplate", Client.PState.PlayerId, capturedMod, capturedId);
                        //CustomNameplateManager.LocalSetNameplate(Client.PState.PlayerId, capturedMod, capturedId);
                        _visible = false;
                    }
                }
                else
                {
                    if (GUI.Button(btnRect, capturedId))
                    {
                        _visible = false;
                    }
                }
            }

            GUI.EndScrollView();
            GUI.DragWindow();
        }
    }
}
