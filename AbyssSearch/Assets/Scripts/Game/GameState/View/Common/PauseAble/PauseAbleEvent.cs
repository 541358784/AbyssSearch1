
public class EventPauseAbleRegister : IEvent
{
    public IPauseAble Obj;

    public EventPauseAbleRegister(IPauseAble obj)
    {
        Obj = obj;
    }
}
public class EventPauseAbleUnRegister : IEvent
{
    public IPauseAble Obj;

    public EventPauseAbleUnRegister(IPauseAble obj)
    {
        Obj = obj;
    }
}