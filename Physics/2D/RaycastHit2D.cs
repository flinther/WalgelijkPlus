using System.Numerics;

namespace WalgelijkPlus.Physics;

public struct RaycastHit2D
{
    public BoxCollider2DComponent Collider;
    public float Distance;
    public Vector2 Normal;
    public Vector2 Point;
}
