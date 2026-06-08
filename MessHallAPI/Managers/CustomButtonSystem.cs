using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MessHallAPI.Debugger;
using MessHallAPI.Config;

namespace MessHallAPI.Managers
{
    public class CustomButtonSystem
    {
        public Action OnPressed;
        public GameObject Target;
        public float ClickRadius = 12.5f;

        private static readonly List<CustomButtonSystem> _buttons = new();
        private static Camera _cam;

        public CustomButtonSystem()
        {
            _buttons.Add(this);
            Logging.DebugLog($"[CustomButtonSystem] Registered button. Total: {_buttons.Count}");
        }

        public void Unregister()
        {
            _buttons.Remove(this);
            Logging.DebugLog($"[CustomButtonSystem] Unregistered button {Target?.name ?? "null"}. Total: {_buttons.Count}");
        }

        public static void OnUpdate()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_cam == null)
                {
                    var camObj = GameObject.Find("3DHUD_Camera");
                    if (camObj == null)
                    {
                        Logging.DebugLog("[CustomButtonSystem] 3DHUD_Camera not found.");
                        return;
                    }
                    _cam = camObj.GetComponent<Camera>();
                    Logging.DebugLog("[CustomButtonSystem] Camera cached.");
                }

                Vector2 mousePos = Mouse.current.position.ReadValue();
                Logging.DebugLog($"[CustomButtonSystem] Click detected at {mousePos}. Checking {_buttons.Count} button(s).");

                foreach (var btn in _buttons)
                {
                    if (btn.Target == null)
                    {
                        Logging.DebugLog("[CustomButtonSystem] Skipping button with null Target.");
                        continue;
                    }

                    if (!btn.Target.activeInHierarchy)
                    {
                        Logging.DebugLog($"[CustomButtonSystem] Skipping inactive button: {btn.Target.name}");
                        continue;
                    }

                    {
                        Vector3 screenMin = _cam.WorldToScreenPoint(btn.Target.transform.position - btn.Target.transform.lossyScale * 0.5f);
                        Vector3 screenMax = _cam.WorldToScreenPoint(btn.Target.transform.position + btn.Target.transform.lossyScale * 0.5f);

                        float xMin = Mathf.Min(screenMin.x, screenMax.x);
                        float yMin = Mathf.Min(screenMin.y, screenMax.y);
                        float xMax = Mathf.Max(screenMin.x, screenMax.x);
                        float yMax = Mathf.Max(screenMin.y, screenMax.y);

                        Rect rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

                        Logging.DebugLog($"[CustomButtonSystem] Button \"{btn.Target.name}\" screen rect: {rect}, mouse: {mousePos}");

                        if (rect.Contains(mousePos))
                        {
                            Logging.DebugLog($"[CustomButtonSystem] Button pressed: {btn.Target.name}");
                            btn.OnPressed?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        public static CustomButtonSystem GetForTarget(GameObject target)
        {
            var result = _buttons.FirstOrDefault(b => b.Target == target);
            Logging.DebugLog($"[CustomButtonSystem] GetForTarget({target?.name ?? "null"}): {(result != null ? "found" : "not found")}");
            return result;
        }
    }
}