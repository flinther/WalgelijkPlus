using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalgelijkPlus;

public struct Mathf
{
    public const double PI = 3.14159265358979323846;

    public static float Min(float a, float b)
    {
        if (a < b)
            return a;
        else
            return b;
    }

    public static float Max(float a, float b)
    {
        if (a < b)
            return b;
        else
            return a;
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max) 
            return max;
        return value;
    }

    public static float Lerp(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
    }

    public static float InverseLerp(float a, float b, float t)
    {
        return (t - a) / (b - a);
    }
}
