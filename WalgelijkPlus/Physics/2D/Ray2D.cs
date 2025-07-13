using System.Numerics;

namespace WalgelijkPlus.Physics;

public struct Ray2D
{
    public Ray2D() { }

    public Ray2D(Vector2 origin, Vector2 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    public Vector2 Origin;
    public Vector2 Direction;

    public Vector2 GetPoint(float distance)
    { 
        return Vector2.Normalize(Origin) + Direction * distance;
    }
}
