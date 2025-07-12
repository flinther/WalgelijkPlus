namespace WalgelijkPlus.Physics;

public struct LayerMask
{
    public LayerMask(int layerMask)
    {
        Layer = layerMask;
    }

    public int Layer;
}
