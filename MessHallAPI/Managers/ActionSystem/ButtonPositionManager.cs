using MessHallAPI.Debugger;
using UnityEngine;
using static MessHallAPI.Config.Settings;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class ButtonPositionManager
    {
        private static readonly Vector3[] SlotPositions = new Vector3[]
        {
            new Vector3( 0.1626f, -0.1088f,  0f),
            new Vector3(-0.1539f, -0.1088f,  0f),
            new Vector3(-0.4995f, -0.1088f,  0f),
            new Vector3( 0.1626f,  0.3016f,  0f),
            new Vector3(-0.1539f,  0.3016f,  0f),
            new Vector3(-0.4995f,  0.3016f,  0f),
            new Vector3(-0.4995f,  0.7120f,  0f),
            new Vector3(-0.1755f,  0.7120f,  0f),
            new Vector3( 0.1485f,  0.7120f,  0f)
        };

        private static readonly Vector3 ActiveScale = new Vector3(0.75f, 0.75f, 1f);

        private const string LowerRightParentPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerRightParent";

        private static readonly string[] ContainerNames = new[]
        {
            "UI_PowerUpIcon",
            "UI_VentButton_3D",
            "UI_TargetedActionIcon",
            "UI_CamsButton_3D"
        };

        private static readonly GameObject?[] _slots = new GameObject?[9];
        private static readonly Dictionary<int, int> _objectToSlot = new();
        private static readonly Dictionary<int, bool> _lastActiveState = new();

        public static void OnUpdate()
        {
            if (!InGame) return;

            var parent = GameObject.Find(LowerRightParentPath);
            if (parent == null) return;

            foreach (var containerName in ContainerNames)
            {
                var container = parent.transform.Find(containerName);
                if (container == null) continue;

                for (int i = 0; i < container.childCount; i++)
                {
                    var child = container.GetChild(i).gameObject;
                    int id = child.GetInstanceID();
                    bool isActive = child.activeSelf;

                    _lastActiveState.TryGetValue(id, out bool wasActive);

                    if (isActive && !wasActive)
                        Activate(child);
                    else if (!isActive && wasActive)
                        Deactivate(child);

                    _lastActiveState[id] = isActive;
                }
            }
        }

        public static void Reset()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = null;

            _objectToSlot.Clear();
            _lastActiveState.Clear();
        }

        private static void Activate(GameObject button)
        {
            int id = button.GetInstanceID();

            if (_objectToSlot.TryGetValue(id, out int existing))
            {
                ApplySlot(button, existing);
                return;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = button;
                    _objectToSlot[id] = i;
                    ApplySlot(button, i);
                    return;
                }
            }

            Logging.Warn($"[ButtonPositionManager] All 9 slots occupied, could not assign slot for {button.name}.");
        }

        private static void Deactivate(GameObject button)
        {
            int id = button.GetInstanceID();

            if (!_objectToSlot.TryGetValue(id, out int slot))
                return;

            _slots[slot] = null;
            _objectToSlot.Remove(id);
        }

        private static void ApplySlot(GameObject button, int slot)
        {
            button.transform.localPosition = SlotPositions[slot];
            button.transform.localScale = ActiveScale;
        }
    }
}