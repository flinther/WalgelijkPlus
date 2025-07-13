using SkiaSharp;
using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace WalgelijkPlus.Graphics;

public class SpriteRenderSystem : Walgelijk.System
{
    public override void Render()
    {
        foreach (var spriteComp in Scene.GetAllComponentsOfType<SpriteAnimatorComponent>())
        {
            if (spriteComp.CurrentAnimation != string.Empty)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(spriteComp.Entity);

                Draw.Reset();
                Draw.Order = spriteComp.RenderOrder;
                var currentTexture = spriteComp.SpriteSheet.Animations[spriteComp.CurrentAnimation].Frames[spriteComp.CurrentFrame];

                Draw.TransformMatrix = spriteComp.Matrix ??
                    Matrix3x2.CreateScale(1, 1) *
                    Matrix3x2.CreateRotation(float.DegreesToRadians(transform.Rotation), transform.Position);

                Vector2 size = Vector2.Zero;
                switch (spriteComp.SpriteRenderMode)
                {
                    case SpriteRenderMode.None:
                        size = currentTexture.Size * SpriteSheet.PixelScaleMultiplier * spriteComp.Scale;
                        break;

                    case SpriteRenderMode.Stretch:
                        size = transform.Scale;
                        break;
                }

                Draw.ScreenSpace = spriteComp.ScreenSpace;
                Draw.BlendMode = spriteComp.BlendMode;
                Draw.Colour = spriteComp.Color;
                Draw.Texture = currentTexture;
                Draw.Quad(new Rect(transform.Position + spriteComp.Offset, size));

                if (!spriteComp.Paused)
                {
                    float frameDuration = 1f / spriteComp.Framerate;
                    spriteComp.FrameTimer += Time.DeltaTime;

                    if (spriteComp.FrameTimer >= frameDuration)
                    {
                        spriteComp.FrameTimer -= frameDuration;

                        spriteComp.CurrentFrame += spriteComp.OffsetFramesBy;

                        var frames = spriteComp.SpriteSheet.Animations[spriteComp.CurrentAnimation].Frames;
                        bool forward = spriteComp.OffsetFramesBy > 0;

                        if (forward && spriteComp.CurrentFrame >= frames.Count ||
                            !forward && spriteComp.CurrentFrame < 0)
                        {
                            if (spriteComp.Loop)
                            {
                                spriteComp.CurrentFrame = forward ? 0 : frames.Count - 1;
                            }
                            else
                            {
                                spriteComp.CurrentFrame -= spriteComp.OffsetFramesBy;
                            }

                            spriteComp.Tree?.OnFinishAnimation();
                        }
                    }
                }
            }
        }
    }
}