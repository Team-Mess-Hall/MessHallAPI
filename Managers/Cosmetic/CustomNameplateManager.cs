using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.XR;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MessHallAPI.Networking;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.Cosmetic
{
    public class CustomNameplateManager
    {
        public static Dictionary<int, Sprite> _nameplates = new Dictionary<int, Sprite>();

        public static Sprite TestNameplate;

        public static void SetNameplate(int playerId, Sprite nameplate)
        {
            _nameplates[playerId] = nameplate;
        }

        public static void ApplyToPlayer(int playerId)
        {
            if (!_nameplates.TryGetValue(playerId, out Sprite nameplate)) return;
            if (!_playerRenderers.TryGetValue(playerId, out MeshRenderer renderer)) return;

            Texture original = renderer.sharedMaterial.mainTexture;

            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0);
            Graphics.Blit(original, rt);
            RenderTexture.active = rt;
            Texture2D edited = new Texture2D(original.width, original.height);
            edited.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
            edited.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            int[] unityYs = { 175, 95, 15 };
            int[] xs = { 944, 1312, 1680 };
            int barW = 351;
            int barH = 65;

            Texture2D patch = ScaleTexture(nameplate.texture, barW, barH);
            Color[] pixels = patch.GetPixels();
            foreach (int y in unityYs)
                foreach (int x in xs)
                    edited.SetPixels(x, y, barW, barH, pixels);
            edited.Apply();
            renderer.material.mainTexture = edited;
        }

        private static Texture2D ScaleTexture(Texture2D source, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(source, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        public static Texture2D GetReadableTexture(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D readable = new Texture2D(source.width, source.height);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return readable;
        }

        private static Dictionary<int, MeshRenderer> _playerRenderers = new Dictionary<int, MeshRenderer>();

        public static void RefreshPlayerAtlases()
        {
            _playerRenderers.Clear();
            int[] playerIdOrder = { 9, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            string basePath = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/UI_Minimap3D/OffsetRotation/BGOffset/Center/PlayerList/PlayersPanel/UI_Moderation/UI_Moderation_PlayerPanel";

            for (int i = 0; i < 10; i++)
            {
                string path = i == 0
                    ? $"{basePath}/SM_ModPlayerPanel_PS"
                    : $"{basePath} ({i})/SM_ModPlayerPanel_PS";

                GameObject panel = GameObject.Find(path);
                if (panel == null) continue;

                MeshRenderer renderer = panel.GetComponent<MeshRenderer>();
                if (renderer == null) continue;

                _playerRenderers[playerIdOrder[i]] = renderer;
            }

            foreach (var playerId in _nameplates.Keys)
                ApplyToPlayer(playerId);
        }

        public static void RefreshMeetingAtlases()
        {
            int[] playerIdOrder = { 9, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            string basePath = "-------- MANAGERS --------/DefaultManagers/VotingManager/VotingUIRoot/MeetingScreen(Clone)/MeetingScreenParent/VotingMain/VotingPlayerLayout_Dynamic/Voting_Player";

            for (int i = 0; i < 10; i++)
            {
                string path = i == 0
                    ? $"{basePath}/SM_UI_PlayerPanel"
                    : $"{basePath} ({i})/SM_UI_PlayerPanel";

                GameObject panel = GameObject.Find(path);
                if (panel == null) continue;

                MeshRenderer renderer = panel.GetComponent<MeshRenderer>();
                if (renderer == null) continue;

                _playerRenderers[playerIdOrder[i]] = renderer;
            }

            foreach (var playerId in _nameplates.Keys)
                ApplyToPlayer(playerId);
        }

        [Obsolete("Load a sprite manually in your mod as this only loads stuff in the api")]
        public static Sprite LoadSpriteFromResource(string resourcePath)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null) return null;

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, bytes);

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100, 0,
                SpriteMeshType.FullRect,
                new Vector4(0, 0, 0, 0),
                false, null
            );
        }

        [MessHallRPC(RPCTarget.All, RPCCaller.Anyone)]
        public static void RPC_SetNameplate([RPCTarget] int target, string modName, string nameplateId)
        {
            Sprite nameplate = NameplateRegistry.Resolve(modName, nameplateId);
            if (nameplate == null) return;
            SetNameplate(target, nameplate);
            RefreshPlayerAtlases();
        }
    }

    public static class NameplateRegistry
    {
        private static Dictionary<string, Dictionary<string, Sprite>> _registry = new();

        public static void Register(string modName, string nameplateId, Sprite sprite)
        {
            if (!_registry.ContainsKey(modName))
                _registry[modName] = new Dictionary<string, Sprite>();
            _registry[modName][nameplateId] = sprite;
        }

        public static Sprite Resolve(string modName, string nameplateId)
        {
            if (_registry.TryGetValue(modName, out var mod))
                if (mod.TryGetValue(nameplateId, out var sprite))
                    return sprite;
            return null;
        }
        public static Dictionary<(string, string), Sprite> GetAll()
        {
            var result = new Dictionary<(string, string), Sprite>();
            foreach (var mod in _registry)
                foreach (var nameplate in mod.Value)
                    result[(mod.Key, nameplate.Key)] = nameplate.Value;
            return result;
        }
    }
}
