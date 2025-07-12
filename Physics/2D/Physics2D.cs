using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Walgelijk;

namespace WalgelijkPlus.Physics;

public static class Physics2D
{
    public static Vector2 Gravity = new Vector2(0, -9.81f);

    public static bool AABBCollision(Scene scene, BoxCollider2DComponent toTest,
        [NotNullWhen(true)] out BoxCollider2DComponent? detected)
    {
        foreach (var collider in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            if (collider != toTest && collider.LayerMask.Layer != toTest.LayerMask.Layer)
                if (toTest.CollidesWith(scene, collider))
                {
                    detected = collider;
                    return true;
                }
        }
        detected = null;
        return false;
    }

    public static void AABBResolve(Scene scene, BoxCollider2DComponent toResolve, BoxCollider2DComponent collider)
    {
        var rb = scene.GetComponentFrom<RigidBody2DComponent>(toResolve.Entity);
        var transform = scene.GetComponentFrom<TransformComponent>(toResolve.Entity);
        var dTransform = scene.GetComponentFrom<TransformComponent>(collider.Entity);

        float dx = dTransform.Position.X - transform.Position.X;
        float dy = dTransform.Position.Y - transform.Position.Y;

        float overlapX = (collider.Size.X / 2 + toResolve.Size.X / 2) - MathF.Abs(dx);
        float overlapY = (collider.Size.Y / 2 + toResolve.Size.Y / 2) - MathF.Abs(dy);

        Vector2 correction = Vector2.Zero;

        if (overlapX < overlapY)
        {
            correction.X = overlapX * MathF.Sign(dx);
        }
        else
        {
            correction.Y = overlapY * MathF.Sign(dy);
        }

        if (correction.X != 0)
            rb.Velocity.X = -rb.Velocity.X * rb.Material.Bounciness;
        else
            rb.Velocity.X *= rb.Material.Friction;
        if (correction.Y != 0)
            rb.Velocity.Y = -rb.Velocity.Y * rb.Material.Bounciness;
        else
            rb.Velocity.Y *= rb.Material.Friction;

        transform.Position -= correction;
    }

    public static bool Raycast(Scene scene, Ray ray, out RaycastHit hitInfo, float maxDistance = float.MaxValue, LayerMask? layerMask = null, IEnumerable<Entity>? ignore = null)
    {
        hitInfo = default;
        bool hitSomething = false;
        float closestDistance = maxDistance;

        foreach (var collider in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            var transform = scene.GetComponentFrom<TransformComponent>(collider.Entity);

            Vector2 boxCenter = transform.Position + collider.Offset;
            Vector2 boxHalfSize = collider.Size / 2;
            Vector2 boxMin = boxCenter - boxHalfSize;
            Vector2 boxMax = boxCenter + boxHalfSize;

            Vector2 invDir = new Vector2(
                1.0f / (ray.Direction.X == 0 ? float.Epsilon : ray.Direction.X),
                1.0f / (ray.Direction.Y == 0 ? float.Epsilon : ray.Direction.Y)
            );

            Vector2 t1 = (boxMin - ray.Origin) * invDir;
            Vector2 t2 = (boxMax - ray.Origin) * invDir;

            float tmin = MathF.Max(MathF.Min(t1.X, t2.X), MathF.Min(t1.Y, t2.Y));
            float tmax = MathF.Min(MathF.Max(t1.X, t2.X), MathF.Max(t1.Y, t2.Y));

            if ((layerMask == null || collider.LayerMask.Layer == layerMask.Value.Layer) && 
                !(ignore?.Contains(collider.Entity) ?? false))
            {
                if (tmax >= 0 && tmin <= tmax && tmin < closestDistance && tmin >= 0)
                {
                    closestDistance = tmin;
                    hitSomething = true;

                    Vector2 point = ray.Origin + ray.Direction * tmin;

                    Vector2 normal = Vector2.Zero;
                    if (tmin == t1.X) normal = new Vector2(-MathF.Sign(ray.Direction.X), 0);
                    else if (tmin == t2.X) normal = new Vector2(MathF.Sign(ray.Direction.X), 0);
                    else if (tmin == t1.Y) normal = new Vector2(0, -MathF.Sign(ray.Direction.Y));
                    else if (tmin == t2.Y) normal = new Vector2(0, MathF.Sign(ray.Direction.Y));

                    hitInfo = new RaycastHit
                    {
                        Point = point,
                        Normal = normal,
                        Distance = tmin,
                        Hit = collider
                    };
                }
            }
        }

        return hitSomething;
    }
}

public struct Ray
{
    public Ray() { }

    public Ray(Vector2 origin, Vector2 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    public Vector2 Origin;
    public Vector2 Direction;
}

public struct RaycastHit
{
    public Vector2 Point;
    public Vector2 Normal;
    public float Distance;

    public BoxCollider2DComponent Hit;
}
