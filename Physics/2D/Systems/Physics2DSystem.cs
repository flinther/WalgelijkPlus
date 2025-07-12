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

        foreach (var rb in Scene.GetAllComponentsOfType<RigidBody2DComponent>())
        {
            var transform = Scene.GetComponentFrom<TransformComponent>(rb.Entity);
            var bc = Scene.GetComponentFrom<BoxCollider2DComponent>(rb.Entity);

            if (rb.BodyType == BodyType.Dynamic)
            {
                rb.Force += Physics2D.Gravity * rb.GravityScale * rb.Mass;

                Vector2 acceleration = rb.Force / rb.Mass;
                rb.Velocity += acceleration * Time.DeltaTime;
                var offset = rb.Velocity * Time.DeltaTime * 60;

                if (!Physics2D.AABBCollision(Scene, bc, out var detected))
                {
                    transform.Position += offset;
                    if (Physics2D.AABBCollision(Scene, bc, out detected))
                        Physics2D.AABBResolve(Scene, bc, detected);
                }
                else
                {
                    Physics2D.AABBResolve(Scene, bc, detected);
                }
            }

            rb.Force = Vector2.Zero;
        }

        foreach (var bc in Scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            var transform = Scene.GetComponentFrom<TransformComponent>(bc.Entity);

            bc.Bounds = new Rect(transform.Position + bc.Offset, bc.Size);

            if (Debug)
            {
                Draw.Reset();
                Draw.Colour = Colors.White.WithAlpha(0);
                Draw.OutlineColour = Colors.Blue;
                Draw.OutlineWidth = 5;

                Draw.Order = RenderOrder.DebugUI;
                Draw.Quad(new Rect(transform.Position + bc.Offset, bc.Size));
            }
        }
    }
}
