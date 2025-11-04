namespace Markdown;

public abstract class Node
{
    public List<Node> Children { get; } = new List<Node>();

}