using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace MessHallAPI.Managers
{
    [Obsolete("this entire class is handled automatically in the API, nothing you need is here.")]
    internal class ModStorage
    {
        public static Sprite ModStamp;

        public static Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MessHallAPI.Assets.ModStamp.png");

        public static Sprite NameplateTest;

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
            rect.anchoredPosition = new Vector2(-10, -10);

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
    }
}
