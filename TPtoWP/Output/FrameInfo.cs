using System.Numerics;

namespace TPtoWP;

public class FrameInfo
{
    public Vector2 TopLeft { get; set; }
    public Vector2 Scale { get; set; }
}

public class SpriteAnimation
{
    public List<FrameInfo> FrameInfos { get; set; } = new List<FrameInfo>();
}

public class SpriteSheet
{
    public static readonly float PixelScaleMultiplier = 5f;

    public string Name;

    public Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();

    public string Texture;
}
