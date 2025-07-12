using System.Numerics;
using Walgelijk;

namespace WalgelijkPlus.Graphics;

[RequiresComponents(typeof(TransformComponent))]
public class SpriteAnimatorComponent : Walgelijk.Component
{
    public SpriteAnimatorComponent(SpriteSheet spriteSheet, Vector2 scale, RenderOrder order)
    {
        SpriteSheet = spriteSheet;
        Scale = scale;
        RenderOrder = order;

        CurrentAnimation = spriteSheet.Animations.First().Key;
    }

    public int CurrentFrame = 0;

    public float Framerate = 12f;

    public string CurrentAnimation { get; private set; } = "Default";

    public float FrameTimer = 0;

    public int OffsetFramesBy = 1;

    public SpriteSheet SpriteSheet { get; private set; }

    public RenderOrder RenderOrder;

    public BlendMode BlendMode;

    public SpriteRenderMode SpriteRenderMode = SpriteRenderMode.None;

    public Vector2 Scale;

    public Matrix3x2? Matrix;

    public bool ScreenSpace = false;

    public Vector2 Offset;

    public Color Color = Colors.White;

    public bool Loop = true;

    public SpriteAnimationTree? Tree = null;

    public bool Paused = false;

    public void SwapCurrentAnimation(string animation, bool playCheck = true, Vector2 offset = default)
    {
        if (CurrentAnimation != animation && playCheck)
        {
            CurrentAnimation = animation;
            CurrentFrame = 0;
            FrameTimer = 0;
            Offset = offset;
        }
        else if (!playCheck)
        {
            CurrentAnimation = animation;
            CurrentFrame = 0;
            FrameTimer = 0;
            Offset = offset;
        }
    }

    public void SwapCurrentSpritesheet(SpriteSheet spritesheet, bool playCheck = true)
    {
        if (SpriteSheet != spritesheet && playCheck)
        {
            SpriteSheet = spritesheet;
            CurrentFrame = 0;
            FrameTimer = 0;
        }
        else if (!playCheck)
        {
            SpriteSheet = spritesheet;
            CurrentFrame = 0;
            FrameTimer = 0;
        }
    }
}

public enum SpriteRenderMode
{
    None,
    Stretch,
    Contain
}

public class SpriteAnimationTree(SpriteAnimatorComponent component)
{
    public string CurrentNode => component.CurrentAnimation;

    private Dictionary<string, Node> Nodes = new();

    public void AddNode(string name, Func<string> next, Vector2 offset = default)
    {
        Nodes.Add(name, new(next, offset));
    }

    public void SetNode(string name, bool playCheck = true)
    {
        if (Nodes.ContainsKey(name))
        {
            component.SwapCurrentAnimation(name, playCheck, Nodes[name].Offset);
        }
    }

    public void OnFinishAnimation()
    {
        if (Nodes[CurrentNode] != null)
        {
            SetNode(Nodes[CurrentNode].Next.Invoke());
        }
    }

    private record class Node(Func<string> Next, Vector2 Offset);
}