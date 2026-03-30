using static MessHallAPI.Base.References;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Config.Settings;
using MessHallAPI.Base;

namespace MessHallAPI.Managers.Cosmetic
{
    public static class NameplateGUI
    {
        private static bool _visible = false;
        private static Vector2 _scrollPos = Vector2.zero;
        private const int WindowId = 9001;
        private static Sprite? Testplate = null;
        public static void OnUpdate()
        {
            if (InGame && GameState.InLobbyState())
            {
                if (Keyboard.current.rightAltKey.wasPressedThisFrame)
                {
                    _visible = !_visible;
                }
                if (Testplate == null)
                {
                    Testplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.2023_indie_bean_nameplate_fight.png");
                    CustomNameplateManager.TestNameplate = CustomNameplateManager.LoadSpriteFromResource("MessHallAPI.Assets.TestNamePlate.png");
                }
                if (Testplate != null)
                {
                    if (NameplateRegistry.GetAll().Count == 0)
                    {
                        NameplateRegistry.Register("MessHallAPI", "TestPlate1", Testplate);
                        NameplateRegistry.Register("MessHallAPI", "TestPlate2", CustomNameplateManager.TestNameplate);
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
                        CustomNameplateManager.RPC_SetNameplate(Client.PState.PlayerId, capturedMod, capturedId);
                        _visible = false;
                    }
                }
                else
                {
                    if (GUI.Button(btnRect, capturedId))
                    {
                        CustomNameplateManager.RPC_SetNameplate(Client.PState.PlayerId, capturedMod, capturedId);
                        _visible = false;
                    }
                }
            }

            GUI.EndScrollView();
            GUI.DragWindow();
        }
    }
}
