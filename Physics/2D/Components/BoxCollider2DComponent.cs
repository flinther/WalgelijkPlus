using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Walgelijk;

namespace WalgelijkPlus.Physics;

[RequiresComponents(typeof(TransformComponent))]
public class BoxCollider2DComponent : Walgelijk.Component
{
    public Vector2 Size;
    public Vector2 Offset;

    public Rect Bounds;

    public LayerMask LayerMask;

    public bool CollidesWith(Scene scene, BoxCollider2DComponent b)
    {
        var transform = scene.GetComponentFrom<TransformComponent>(Entity);
        var bTransform = scene.GetComponentFrom<TransformComponent>(b.Entity);

        float ax = transform.Position.X + Offset.X + (Size.X / 2);
        float ay = transform.Position.Y + Offset.Y + (Size.Y / 2);
        float bx = bTransform.Position.X + Offset.X + (Size.X / 2);
        float by = bTransform.Position.Y + Offset.Y + (Size.Y / 2);

        if (Math.Abs(ax - bx) < (Size.X / 2) + (b.Size.X / 2))
        {
            if (Math.Abs(ay - by) < (Size.Y / 2) + (b.Size.Y / 2))
            {
                return true;
            }
        }

        return false;
    }
}