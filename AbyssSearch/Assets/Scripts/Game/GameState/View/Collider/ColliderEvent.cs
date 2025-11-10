public class EventColliderLocalPositionChange : IEvent
{
    public Collider Collider;

    public EventColliderLocalPositionChange(Collider collider)
    {
        Collider = collider;
    }
}

public class EventColliderTrigger : IEvent
{
    public Collider A;
    public Collider B;

    public EventColliderTrigger(Collider a,Collider b)
    {
        A = a;
        B = b;
    }
}