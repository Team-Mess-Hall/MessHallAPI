using MessHallAPI.Networking;
using MessHallAPI.Managers.Cosmetic;
using System.Reflection;
using UnityEngine;

namespace Example.Managers
{
    public class ExampleNameplateManager
    {
        public static Sprite ExampleSprite;
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
        public static void RegisterNameplates()
        {
            if (!NameplateRegistry.GetAll().Values.Contains(ExampleSprite) && ExampleSprite != null)
            {
                NameplateRegistry.Register("Example", "ExamplePlate1", ExampleSprite);
            }
        }
        public override void OnUpdate()
        {
            if (ExampleSprite == null)
            {
                LoadSpriteFromResource("Example.Assets.ExamplePlate1."); // Requires it to be an embedded resource. See nameplates.md
            }
        }
    }
}
