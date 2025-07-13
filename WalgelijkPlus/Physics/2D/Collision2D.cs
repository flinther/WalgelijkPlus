using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WalgelijkPlus.Physics;

public struct Collision2D
{
    public BoxCollider2DComponent Collider;
    public BoxCollider2DComponent OtherCollider;
    public Vector2 RelativeVelocity;
}
