using System.Numerics;

namespace WalgelijkPlus.Graphics;

public class SpriteAnimationTree(SpriteAnimatorComponent component)
{
    public string CurrentNode => component.CurrentAnimation;

    private Dictionary<string, Node> Nodes = new();

    public void AddNode(string name, Func<string> next, Vector2 offset = default)
    {
        Nodes.Add(name, new(next, offset));
    }

    public void SetNode(string name, bool playCheck = true)
    {
        if (Nodes.ContainsKey(name))
        {
            component.SwapCurrentAnimation(name, playCheck, Nodes[name].Offset);
        }
    }

    public void OnFinishAnimation()
    {
        if (Nodes[CurrentNode] != null)
        {
            SetNode(Nodes[CurrentNode].Next.Invoke());
        }
    }

    private record class Node(Func<string> Next, Vector2 Offset);
}