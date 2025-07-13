namespace WalgelijkPlus.Physics;

public struct LayerMask
{
    public LayerMask(int layerMask)
    {
        Layer = layerMask;
        ToIgnore = new();
    }

    public LayerMask(int layerMask, List<LayerMask> toIgnore)
    {
        Layer = layerMask;
        ToIgnore = toIgnore;
    }

    public int Layer;

    public List<LayerMask> ToIgnore;
}
