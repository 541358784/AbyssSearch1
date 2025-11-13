using System.Collections.Generic;

public class PauseAbleController:Singleton<PauseAbleController>
{
    private readonly HashSet<IPauseAble> pauseables = new();
    public PauseAbleController()
    {
        EventManager.Instance.AddEvent<EventPauseAbleRegister>(Register);
        EventManager.Instance.AddEvent<EventPauseAbleUnRegister>(UnRegister);
    }
    public void Register(EventPauseAbleRegister evt)
    {
        pauseables.Add(evt.Obj);
    }
    public void UnRegister(EventPauseAbleUnRegister evt)
    {
        pauseables.Remove(evt.Obj);
    }
    public void LogicUpdateAll()
    {
        foreach (var p in pauseables)
            p.LogicUpdate();
    }
}