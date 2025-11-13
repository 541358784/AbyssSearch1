using System.Collections.Generic;

public class ColliderCheckGrid
{
    public Dictionary<ColliderType,LinkedList<ICollider>> Contains;
    public int IndexX;
    public int IndexY;
    public ColliderCheckGrid(int x,int y)
    {
        IndexX = x;
        IndexY = y;
        Contains = new Dictionary<ColliderType, LinkedList<ICollider>>();
    }

    public void Remove(ICollider collider)
    {
        if (!Contains.ContainsKey(collider.Type))
            return;
        Contains[collider.Type].Remove(collider);
    }
    public void Add(ICollider collider)
    {
        if (!Contains.ContainsKey(collider.Type))
        {
            Contains.Add(collider.Type,new LinkedList<ICollider>());
        }
        Contains[collider.Type].AddLast(collider);
    }

    public IEnumerable<ICollider> GetContains(ColliderType type)
    {
        foreach (var pair in Contains)
        {
            if (type.HasFlag(pair.Key))
            {
                foreach (var collider in pair.Value)
                {
                    yield return collider;
                }
            }
        }
    }
}