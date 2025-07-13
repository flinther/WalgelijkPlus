using System.Numerics;
using Walgelijk;

namespace WalgelijkPlus.Physics;

[RequiresComponents(typeof(TransformComponent), typeof(BoxCollider2DComponent))]
public class RigidBody2DComponent : Walgelijk.Component
{
    public Vector2 Velocity;
    public Vector2 Force;

    public PhysicsMaterial Material;

    public BodyType BodyType = BodyType.Dynamic;
    public float Mass = 1;
    public float GravityScale = 1;

    public void AddForce(Vector2 force)
    {
        Force += force;
    }
}