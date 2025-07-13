using Newtonsoft.Json;
using Walgelijk;

namespace WalgelijkPlus.Graphics;

public class SpriteAnimation
{
    public SpriteAnimation(List<FrameInfo> frameInfos)
    {
        FrameInfos = frameInfos;
    }

    public List<FrameInfo> FrameInfos { get; set; } = new List<FrameInfo>();

    [JsonIgnore]
    public List<IReadableTexture> Frames { get; set; } = new List<IReadableTexture>();
}