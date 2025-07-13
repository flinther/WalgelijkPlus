using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace WalgelijkPlus.Physics;

public class Physics2DSystem : Walgelijk.System
{
    public static bool Debug = false;

    public override void FixedUpdate()
    {
        if (Input.IsKeyPressed(Key.F3))
            Debug = !Debug;

        Physics2D.Simulate(Scene, Time.DeltaTime);
    }

    public override void Render()
    {
        foreach (var bc in Scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            var transform = Scene.GetComponentFrom<TransformComponent>(bc.Entity);
            if (Debug)
            {
                Draw.Reset();
                Draw.Colour = Colors.White.WithAlpha(0);
                Draw.OutlineColour = Colors.Blue;
                Draw.OutlineWidth = 5;

                Draw.Order = RenderOrder.Top;
                Draw.Quad(new Rect(transform.Position + bc.Offset, bc.Size));
            }
        }
    }
}
