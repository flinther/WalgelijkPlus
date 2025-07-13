using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WalgelijkPlus.Physics;

public struct PhysicsMaterial
{
    public PhysicsMaterial()
    {
    }

    public PhysicsMaterial(float bounciness)
    {
        Bounciness = bounciness;
    }

    public PhysicsMaterial(float bounciness, float friction)
    {
        Bounciness = bounciness;
        Friction = friction;
    }

    public PhysicsMaterial(float bounciness, float friction, uint layer)
    {
        Bounciness = bounciness;
        Friction = friction;
        CollisionLayer = layer;
    }

    public float Bounciness;
    public float Friction;

    public uint CollisionLayer;
}