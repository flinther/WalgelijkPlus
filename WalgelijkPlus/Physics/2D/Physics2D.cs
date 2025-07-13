using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Walgelijk;

namespace WalgelijkPlus.Physics;

/// <summary>
/// Standard 2D AABB physics.
/// </summary>
public static class Physics2D
{
    public static Vector2 Gravity = new Vector2(0, -9.81f);

    public static bool OverlapBox(Scene scene, BoxCollider2DComponent toTest, out Collision2D collision)
    {
        collision = default;
        foreach (var collider in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            if (collider != toTest && collider.LayerMask.Layer != toTest.LayerMask.Layer)
                if (toTest.CollidesWith(scene, collider))
                {
                    collision = new Collision2D
                    {
                        Collider = collider,
                        OtherCollider = toTest
                    };
                    return true;
                }
        }
        return false;
    }

    public static bool OverlapBoxAll(Scene scene, BoxCollider2DComponent toTest, out Collision2D[] detected)
    {
        List<Collision2D> list = new();
        detected = default;
        bool hitSomething = false;
        foreach (var collider in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            if (collider != toTest && collider.LayerMask.Layer != toTest.LayerMask.Layer)
                if (toTest.CollidesWith(scene, collider))
                {
                    list.Add(new Collision2D
                    {
                        Collider = collider,
                        OtherCollider = toTest
                    });
                    hitSomething = true;
                }
        }

        detected = list.ToArray();
        return hitSomething;
    }

    public static void OverlapBoxResolve(Scene scene, Collision2D collision)
    {
        var rb = scene.GetComponentFrom<RigidBody2DComponent>(collision.OtherCollider.Entity);
        var transform = scene.GetComponentFrom<TransformComponent>(collision.OtherCollider.Entity);
        var dTransform = scene.GetComponentFrom<TransformComponent>(collision.Collider.Entity);

        float dx = dTransform.Position.X - transform.Position.X;
        float dy = dTransform.Position.Y - transform.Position.Y;

        float overlapX = (collision.Collider.Size.X / 2 + collision.OtherCollider.Size.X / 2) - MathF.Abs(dx);
        float overlapY = (collision.Collider.Size.Y / 2 + collision.OtherCollider.Size.Y / 2) - MathF.Abs(dy);

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

    public static void OverlapBoxResolveAll(Scene scene, Collision2D[] collisions)
    {
        Vector2 totalCorrection = Vector2.Zero;
        var rb = scene.GetComponentFrom<RigidBody2DComponent>(collisions[0].OtherCollider.Entity);
        var tf = scene.GetComponentFrom<TransformComponent>(collisions[0].OtherCollider.Entity);
        
        foreach (var collision in collisions)
        {
            var transform = scene.GetComponentFrom<TransformComponent>(collision.OtherCollider.Entity);
            var dTransform = scene.GetComponentFrom<TransformComponent>(collision.Collider.Entity);

            float dx = dTransform.Position.X - transform.Position.X;
            float dy = dTransform.Position.Y - transform.Position.Y;

            float overlapX = (collision.Collider.Size.X / 2 + collision.OtherCollider.Size.X / 2) - MathF.Abs(dx);
            float overlapY = (collision.Collider.Size.Y / 2 + collision.OtherCollider.Size.Y / 2) - MathF.Abs(dy);

            Vector2 correction = Vector2.Zero;

            if (overlapX < overlapY)
            {
                correction.X = overlapX * MathF.Sign(dx);
            }
            else
            {
                correction.Y = overlapY * MathF.Sign(dy);
            }

            totalCorrection += correction;
        }

        if (totalCorrection.X != 0)
            rb.Velocity.X = -rb.Velocity.X * rb.Material.Bounciness;
        else
            rb.Velocity.X *= rb.Material.Friction;
        if (totalCorrection.Y != 0)
            rb.Velocity.Y = -rb.Velocity.Y * rb.Material.Bounciness;
        else
            rb.Velocity.Y *= rb.Material.Friction;

        tf.Position -= totalCorrection;
    }

    public static bool Raycast(Scene scene, Ray2D ray, out RaycastHit2D hitInfo, float maxDistance = float.MaxValue, LayerMask? layerMask = null, IEnumerable<Entity>? ignore = null)
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

                    hitInfo = new RaycastHit2D
                    {
                        Collider = collider,
                        Distance = tmin,
                        Normal = normal,
                        Point = point
                    };
                }
            }
        }

        return hitSomething;
    }

    public static bool RaycastAll(Scene scene, Ray2D ray, out RaycastHit2D[] hitInfo, float maxDistance = float.MaxValue, LayerMask? layerMask = null, IEnumerable<Entity>? ignore = null)
    {
        List<RaycastHit2D> list = new();
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

                    list.Add(new RaycastHit2D
                    {
                        Collider = collider,
                        Distance = tmin,
                        Normal = normal,
                        Point = point
                    });
                }
            }
        }

        hitInfo = list.ToArray();
        return hitSomething;
    }

    public static bool BoxCast(Scene scene, Vector2 origin, Vector2 size, Vector2 direction, out RaycastHit2D hitInfo, float maxDistance = float.MaxValue, LayerMask? layerMask = null, IEnumerable<Entity>? ignore = null)
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

            Vector2 expandedMin = boxMin - (size / 2);
            Vector2 expandedMax = boxMax + (size / 2);

            Ray2D ray = new Ray2D(origin, Vector2.Normalize(direction));

            Vector2 invDir = new Vector2(
                1.0f / (ray.Direction.X == 0 ? float.Epsilon : ray.Direction.X),
                1.0f / (ray.Direction.Y == 0 ? float.Epsilon : ray.Direction.Y)
            );

            Vector2 t1 = (expandedMin - ray.Origin) * invDir;
            Vector2 t2 = (expandedMax - ray.Origin) * invDir;

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

                    hitInfo = new RaycastHit2D
                    {
                        Collider = collider,
                        Distance = tmin,
                        Normal = normal,
                        Point = point
                    };
                }
            }
        }

        return hitSomething;
    }

    public static bool BoxCastAll(Scene scene, Vector2 origin, Vector2 size, Vector2 direction, out RaycastHit2D[] hitInfo, float maxDistance = float.MaxValue, LayerMask? layerMask = null, IEnumerable<Entity>? ignore = null)
    {
        List<RaycastHit2D> list = new();
        bool hitSomething = false;
        float closestDistance = maxDistance;

        foreach (var collider in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            var transform = scene.GetComponentFrom<TransformComponent>(collider.Entity);

            Vector2 boxCenter = transform.Position + collider.Offset;
            Vector2 boxHalfSize = collider.Size / 2;
            Vector2 boxMin = boxCenter - boxHalfSize;
            Vector2 boxMax = boxCenter + boxHalfSize;

            Vector2 expandedMin = boxMin - (size / 2);
            Vector2 expandedMax = boxMax + (size / 2);

            Ray2D ray = new Ray2D(origin, Vector2.Normalize(direction));

            Vector2 invDir = new Vector2(
                1.0f / (ray.Direction.X == 0 ? float.Epsilon : ray.Direction.X),
                1.0f / (ray.Direction.Y == 0 ? float.Epsilon : ray.Direction.Y)
            );

            Vector2 t1 = (expandedMin - ray.Origin) * invDir;
            Vector2 t2 = (expandedMax - ray.Origin) * invDir;

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

                    list.Add(new RaycastHit2D
                    {
                        Collider = collider,
                        Distance = tmin,
                        Normal = normal,
                        Point = point
                    });
                }
            }
        }

        hitInfo = list.ToArray();

        return hitSomething;
    }

    public static bool Simulate(Scene scene, float deltaTime, LayerMask? simulationLayers = null)
    {
        foreach (var rb in scene.GetAllComponentsOfType<RigidBody2DComponent>())
        {
            var transform = scene.GetComponentFrom<TransformComponent>(rb.Entity);
            var bc = scene.GetComponentFrom<BoxCollider2DComponent>(rb.Entity);

            if (rb.BodyType == BodyType.Dynamic)
            {
                rb.Force += Gravity * rb.GravityScale * rb.Mass;

                Vector2 acceleration = rb.Force / rb.Mass;
                rb.Velocity += acceleration * deltaTime;
                var offset = rb.Velocity * deltaTime * 60;

                if (!OverlapBoxAll(scene, bc, out var collision))
                {
                    transform.Position += offset;
                    if (OverlapBoxAll(scene, bc, out collision))
                        OverlapBoxResolveAll(scene, collision);
                }
                else
                {
                    OverlapBoxResolveAll(scene, collision);
                }
            }

            rb.Force = Vector2.Zero;
        }

        foreach (var bc in scene.GetAllComponentsOfType<BoxCollider2DComponent>())
        {
            var transform = scene.GetComponentFrom<TransformComponent>(bc.Entity);

            bc.Bounds = new Rect(transform.Position + bc.Offset, bc.Size);
        }

        return true;
    }
}