using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MessHallAPI.Debugger;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using static MessHallAPI.Base.References;
using static MessHallAPI.Config.Settings;
using static MessHallAPI.Managers.KeybindManager;

namespace MessHallAPI.Managers.ActionSystem
{
    public static class PowerRegistration
    {
        private static readonly Dictionary<PowerUps, (PowerUp Definition, UIInteractButton? Button, Action<int> OnUse, Action? OnUpdate, string PowerName, Func<Sprite> PowerIcon, bool IsTargeted, string Keybind)> _powers = new(); private static PlayerState Caller = null!;
        private static PlayerState Target = null!;

        public static string PowerNameLabel = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerRightParent/UI_PowerUpIcon/SM_PowerUp_256_Button/NameLabel";
        public static string PowerMeshRenderer = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerRightParent/UI_PowerUpIcon/SM_PowerUp_256_Button";

        private const int AtlasCellSize = 256;
        private const int AtlasWidth = 2048;
        private const int AtlasHeight = 2048;

        private static int _nextPowerId = 256;

        public static PowerUps AllocatePower()
        {
            var allocated = (PowerUps)_nextPowerId;
            _nextPowerId <<= 1;
            return allocated;
        }

        public static void Register(PowerUps power, PowerUp definition, Action onUse, string powerName, Func<Sprite> powerIcon, string keybind = "E", Action? onUpdate = null)
        {
            if (!_powers.TryAdd(power, (definition, null, _ => onUse(), onUpdate, powerName, powerIcon, false, keybind)))
            {
                Logging.Warn($"PowerRegistration: Power {power} is already registered, skipping.");
                return;
            }

            Logging.Log($"PowerRegistration: Registered power {power}.");
        }

        public static void RegisterTargeted(PowerUps power, PowerUp definition, Action<int> onUse, string powerName, Func<Sprite> powerIcon, string keybind = "E", Action? onUpdate = null)
        {
            if (!_powers.TryAdd(power, (definition, null, onUse, onUpdate, powerName, powerIcon, true, keybind)))
            {
                Logging.Warn($"PowerRegistration: Targeted power {power} already registered.");
                return;
            }

            Logging.Log($"PowerRegistration: Registered targeted power {power}.");
        }

        public static void Unregister(PowerUps power)
        {
            _powers.Remove(power);
        }

        public static List<PowerUp> CreateAll()
        {
            var powers = new List<PowerUp>();

            foreach (var (power, entry) in _powers)
            {
                powers.Add(entry.Definition);
                Logging.DebugLog($"PowerRegistration: Created power {power}.");
            }

            return powers;
        }

        public static void Dispatch(PowerUps power, int CallerPlayerID)
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId == CallerPlayerID)
                    Caller = player;
            }

            if (_powers.TryGetValue(power, out var entry))
            {
                entry.OnUse.Invoke(CallerPlayerID);
            }
            else
            {
                Logging.Warn($"PowerRegistration: No handler registered for power {power}.");
            }
        }

        public static void DispatchTarget(PowerUps power, int CallerPlayerID, int TargetID)
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.PlayerId == CallerPlayerID)
                    Caller = player;

                if (player.PlayerId == TargetID)
                    Target = player;
            }

            if (_powers.TryGetValue(power, out var entry))
            {
                entry.OnUse.Invoke(TargetID);
                Caller.ActivePowerUps = PowerUps.None;
            }
            else
            {
                Logging.Warn($"PowerRegistration: No targeted handler registered for power {power}.");
            }
        }

        public static void OnUpdate()
        {
            try
            {
                if (!InGame)
                    return;

                if (Client.PState == null)
                    return;

                var active = Client.PState.ActivePowerUps;

                if (!CustomPowerHandler.IsCustomPower((int)active))
                    return;

                if (!_powers.TryGetValue(active, out var entry))
                    return;

                var iconObj = GameObject.Find($"{IconParentPath}/SM_PowerUp_{active}_Button");
                if (iconObj != null)
                {
                    var mr = iconObj.GetComponent<MeshRenderer>();
                    if (mr != null && mr.material.mainTexture == null)
                    {
                        var sprite = entry.PowerIcon();
                        if (sprite != null)
                        {
                            var rt = RenderTexture.GetTemporary(16, 16, 0, RenderTextureFormat.ARGB32);
                            Graphics.Blit(sprite.texture, rt);
                            var prev = RenderTexture.active;
                            RenderTexture.active = rt;
                            var scaled = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                            scaled.ReadPixels(new Rect(0, 0, 16, 16), 0, 0);
                            scaled.Apply();
                            RenderTexture.active = prev;
                            RenderTexture.ReleaseTemporary(rt);

                            mr.material.mainTexture = scaled;
                            mr.material.mainTextureScale = new Vector2(1f, 1f);
                            mr.material.mainTextureOffset = new Vector2(0f, 0f);
                            Logging.DebugLog($"PowerRegistration: Re-referenced texture for power {active}.");
                        }
                    }

                    var bindingGlyph = iconObj.transform.Find("3D_BindingGlyph");
                    if (bindingGlyph != null)
                    {
                        var glyphKBM = bindingGlyph.Find("GlyphKBM");
                        if (glyphKBM != null)
                        {
                            var tile = glyphKBM.GetComponent<LUITile>();
                            if (tile != null)
                                tile.SetTileOffset(StringToV2(entry.Keybind));
                        }
                    }
                }

                if (!GameState.InTaskState())
                    return;

                if (entry.Definition.TargetedAction != ProximityTargetedAction.None)
                {
                    TargetOutlineManager.OutlineTarget(
                        entry.Definition.TargetedAction,
                        entry.Definition.TargetedAction,
                        entry.Definition.Duration,
                        entry.Button
                    );
                }

                if (!Keyboard.current.allKeys[StringToKeybind(entry.Keybind.ToLower())].wasPressedThisFrame)
                    return;

                if (entry.IsTargeted)
                {
                    if (Killing._targetPlayers.Count == 0)
                        return;

                    var target = Killing._targetPlayers[0].PlayerId;

                    CustomPowerHandler.RPC_UsePowerTarget(
                        (int)active,
                        Client.PState.PlayerId,
                        target
                    );
                }
                else
                {
                    CustomPowerHandler.RPC_UsePower(
                        (int)active,
                        Client.PState.PlayerId
                    );
                }
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }

        private static Texture2D PatchAtlas(Texture2D? originalAtlas, Sprite sprite, int col, int row)
        {
            Texture2D atlas;

            if (originalAtlas != null)
            {
                atlas = new Texture2D(originalAtlas.width, originalAtlas.height, originalAtlas.format, false);
                Graphics.CopyTexture(originalAtlas, atlas);
            }
            else
            {
                atlas = new Texture2D(AtlasWidth, AtlasHeight, TextureFormat.RGBA32, false);
            }

            var src = sprite.texture;
            Texture2D cell;

            if (src.width == AtlasCellSize && src.height == AtlasCellSize)
            {
                cell = src;
            }
            else
            {
                var rt = RenderTexture.GetTemporary(AtlasCellSize, AtlasCellSize, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(src, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                cell = new Texture2D(AtlasCellSize, AtlasCellSize, TextureFormat.RGBA32, false);
                cell.ReadPixels(new Rect(0, 0, AtlasCellSize, AtlasCellSize), 0, 0);
                cell.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }

            int destX = col * AtlasCellSize;
            int destY = row * AtlasCellSize;

            atlas.SetPixels(destX, destY, AtlasCellSize, AtlasCellSize, cell.GetPixels());
            atlas.Apply();

            return atlas;
        }

        private const string IconParentPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerRightParent/UI_PowerUpIcon";

        private const string DisinfectPath =
            IconParentPath + "/SM_PowerUp_Disinfect_Button";

        public static void BuildIcon(
            PowerUps power,
            (PowerUp Definition, UIInteractButton? Button, Action<int> OnUse, Action? OnUpdate, string PowerName, Func<Sprite> PowerIcon, bool IsTargeted, string Keybind) entry,
            GameObject disinfectObj,
            PowerUpIconParent iconParent)
        {
            var cloned = GameObject.Instantiate(disinfectObj, disinfectObj.transform.parent);
            cloned.name = $"SM_PowerUp_{power}_Button";

            var button = cloned.GetComponent<UIInteractButton>();
            if (button == null)
            {
                Logging.Error($"PowerRegistration: Cloned object for {power} has no UIInteractButton.");
                return;
            }

            var mr = cloned.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                Logging.Error($"PowerRegistration: Cloned object for {power} has no MeshRenderer.");
                return;
            }

            var sprite = entry.PowerIcon();
            if (sprite != null)
            {
                var rt = RenderTexture.GetTemporary(16, 16, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(sprite.texture, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var scaled = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                scaled.ReadPixels(new Rect(0, 0, 16, 16), 0, 0);
                scaled.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);

                mr.material.mainTexture = scaled;
                mr.material.mainTextureScale = new Vector2(1f, 1f);
                mr.material.mainTextureOffset = new Vector2(0f, 0f);
            }
            else
            {
                Logging.Warn($"PowerRegistration: PowerIcon returned null for power: {power}, icon will be empty.");
            }

            if (!IsKeyAccepted(entry.Keybind))
            {
                Logging.Error($"{entry.Keybind} is not a valid key, please refer to KeybindManager for valid keys; Falling back to Default");
                entry = entry with { Keybind = eKey };
            }

            var label = cloned.transform.Find("NameLabel");
            if (label != null)
            {
                var text = label.GetComponent<TextMeshPro>();
                if (text != null)
                    text.text = entry.PowerName;
            }

            iconParent._icons.Add(new PowerUpIconParent.Icon()
            {
                Type = power,
                Button = button
            });

            _powers[power] = entry with { Button = button };

            Logging.DebugLog($"PowerRegistration: Built icon for power {power}.");
        }

        public static void BuildIcons()
        {
            var parentObj = GameObject.Find(IconParentPath);

            if (parentObj == null)
            {
                Logging.Error("PowerRegistration: Could not find icon parent.");
                return;
            }

            var iconParent = parentObj.GetComponent<PowerUpIconParent>();

            if (iconParent == null)
            {
                Logging.Error("PowerRegistration: Icon parent has no PowerUpIconParent component.");
                return;
            }

            var disinfectObj = GameObject.Find(DisinfectPath);

            if (disinfectObj == null)
            {
                Logging.Error("PowerRegistration: Could not find Disinfect button to clone.");
                return;
            }

            foreach (var (power, entry) in _powers)
            {
                BuildIcon(power, entry, disinfectObj, iconParent);
            }
        }

        public static void AutoRegister()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = type.GetCustomAttribute<PowerUpDefinitionAttribute>();
                if (attr == null) continue;

                if (typeof(IPowerUpHandler).IsAssignableFrom(type))
                {
                    var handler = (IPowerUpHandler)Activator.CreateInstance(type);
                    var allocated = AllocatePower();
                    if (handler is CustomPower cp) cp.AllocatedType = allocated;
                    Register(allocated, handler.Definition, handler.OnUse, handler.PowerName, () => handler.PowerIcon, handler.Keybind, handler.OnUpdate);
                    continue;
                }

                if (typeof(ITargetedPowerHandler).IsAssignableFrom(type))
                {
                    var handler = (ITargetedPowerHandler)Activator.CreateInstance(type);
                    var allocated = AllocatePower();
                    if (handler is CustomTargetedPower ctp) ctp.AllocatedType = allocated;
                    RegisterTargeted(allocated, handler.Definition, handler.OnUseTarget, handler.PowerName, () => handler.PowerIcon, handler.Keybind, handler.OnUpdate);
                    continue;
                }

                Logging.Warn($"PowerRegistration: {type.Name} has [PowerUpDefinition] but no valid handler interface.");
            }
        }
    }
}