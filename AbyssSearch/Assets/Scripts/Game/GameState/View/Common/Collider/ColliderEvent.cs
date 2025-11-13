public class EventColliderPositionChange : IEvent
{
    public ICollider Collider;

    public EventColliderPositionChange(ICollider collider)
    {
        Collider = collider;
    }
}

public class EventColliderTrigger : IEvent
{
    public ICollider A;
    public ICollider B;

    public EventColliderTrigger(ICollider a,ICollider b)
    {
        A = a;
        B = b;
    }
}

public class EventColliderRegister : IEvent
{
    public ICollider Obj;

    public EventColliderRegister(ICollider obj)
    {
        Obj = obj;
    }
}
public class EventColliderUnRegister : IEvent
{
    public ICollider Obj;

    public EventColliderUnRegister(ICollider obj)
    {
        Obj = obj;
    }
}