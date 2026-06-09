using Il2CppSG.Airlock.Audio;
using Il2CppSG.Airlock.Graphics;
using Il2CppSG.Airlock.Timeline;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace MessHallAPI.Managers
{
    [Obsolete("this entire class is handled automatically in the API, nothing you need is here.")]
    public class ModStorage
    {
        public static Sprite ModStamp;
        public static Sprite CloseButton;
        public static Sprite DeputyIcon;
        public static Sprite EngineerIcon;
        public static Sprite GuardianAngelIcon;
        public static Sprite ImpostorIcon;
        public static Sprite InfectedIcon;
        public static Sprite LeftArrow;
        public static Sprite RightArrow;
        public static Sprite WraithIcon;
        public static Sprite ScannerIcon;
        public static Sprite TrackerIcon;
        public static Sprite VigilanteIcon;
        public static Sprite RoleButton;

        public static Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MessHallAPI.Assets.ModStamp.png");

        public static Sprite NameplateTest;

        public static Texture2D MapTest = null!;

        public static void LoadModStamp()
        {
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            Texture2D modStamp = new Texture2D(1, 1);

            ImageConversion.LoadImage(modStamp, bytes);

            ModStamp = Sprite.Create(modStamp, new Rect(0,0,modStamp.width,modStamp.height), new Vector2(1,1), 100, 0, SpriteMeshType.FullRect, new Vector4(0,0,0,0), false, null);

            AddGlobalModStamp();
        }
        public static void AddGlobalModStamp()
        {
            GameObject stampobj = new GameObject();
            stampobj.name = "ModStamp";
            Canvas canvas = stampobj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            stampobj.AddComponent<CanvasScaler>();
            stampobj.AddComponent<GraphicRaycaster>();

            GameObject imageobj = new GameObject();
            imageobj.name = "StampImage";
            imageobj.transform.SetParent(stampobj.transform);

            Image image = imageobj.AddComponent<Image>();
            image.sprite = ModStamp;

            RectTransform rect = imageobj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20f, -20f);

            DontDestroyOnLoad(stampobj);
        }
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

        public static Texture2D LoadTextureFromResource(string resourcePath)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null) return null;

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, bytes);

            return tex;
        }

        public static AudioClip LoadAudio(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream(name);

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadBytes(4);
            reader.ReadInt32();
            reader.ReadBytes(4);

            reader.ReadBytes(4);
            int fmtSize = reader.ReadInt32();
            reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize > 16) reader.ReadBytes(fmtSize - 16);

            string chunkId = "";
            int chunkSize = 0;
            while (chunkId != "data")
            {
                chunkId = new string(reader.ReadChars(4));
                chunkSize = reader.ReadInt32();
                if (chunkId != "data") reader.ReadBytes(chunkSize);
            }

            byte[] rawSamples = reader.ReadBytes(chunkSize);
            int bytesPerSample = bitDepth / 8;
            int sampleCount = rawSamples.Length / bytesPerSample;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = bitDepth switch
                {
                    8 => (rawSamples[i] - 128) / 128f,
                    16 => BitConverter.ToInt16(rawSamples, i * 2) / 32768f,
                    24 => (rawSamples[i * 3] | (rawSamples[i * 3 + 1] << 8) | ((sbyte)rawSamples[i * 3 + 2] << 16)) / 8388608f,
                    32 => BitConverter.ToSingle(rawSamples, i * 4),
                    _ => throw new NotSupportedException($"Unsupported bit depth: {bitDepth}")
                };
            }

            AudioClip clip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static void LoadIcons()
        {
            CloseButton = LoadSpriteFromResource("MessHallAPI.Assets.CloseButton.png");
            WraithIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Wraith_Icon.png");
            LeftArrow = LoadSpriteFromResource("MessHallAPI.Assets.LeftArrow.png");
            RightArrow = LoadSpriteFromResource("MessHallAPI.Assets.RightArrow.png");
            ImpostorIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Impostor_Icon.png");
            VigilanteIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Vigilante_Icon.png");
            DeputyIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Deputy_Icon.png");
            InfectedIcon = LoadSpriteFromResource("MessHallAPI.Assets.InfectedIcon.png");
            ScannerIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Scanner_Icon.png");
            GuardianAngelIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Guardian_Angel_Icon.png");
            TrackerIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Tracker_Icon.png");
            EngineerIcon = LoadSpriteFromResource("MessHallAPI.Assets.3D_Engineer_Icon.png");
            RoleButton = LoadSpriteFromResource("MessHallAPI.Assets.3D_Roles_button.png");
        }
    }
}
