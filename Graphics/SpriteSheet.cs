using Newtonsoft.Json;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;

namespace WalgelijkPlus.Graphics;

public class SpriteSheet
{
    public static readonly float PixelScaleMultiplier = 3f;

    public SpriteSheet(AssetRef<Texture> texture, string name)
    {
        Texture = texture;
        Name = name;
    }

    public string Name;

    public Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();

    public AssetRef<Texture> Texture;

    public class AssetDeserializer : IAssetDeserialiser<SpriteSheet>
    {
        public SpriteSheet Deserialise(Func<Stream> stream, in AssetMetadata assetMetadata)
        {
            using var reader = new StreamReader(stream());
            var data = reader.ReadToEnd();
            var obj = JsonConvert.DeserializeObject<SpriteSheet>(data) ?? throw new Exception("Attempt to deserialise null spritesheet");
            foreach (var anim in obj.Animations.Values)
                anim.Frames = anim.FrameInfos.Select(info => TextureEdit.GetSpriteRegion(info.TopLeft, info.Scale, obj.Texture.Value)).ToList();

            return obj;
        }

        public bool IsCandidate(in AssetMetadata assetMetadata)
        {
            return assetMetadata.Path.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public static readonly SpriteSheet White = Assets.Load<SpriteSheet>("atlases/white.json");
}