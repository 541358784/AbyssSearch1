public interface IPauseAble
{
    public void LogicUpdate();
    
    public void PauseAbleRegister()//注册Update
    {
        EventManager.Instance.SendEventImmediately(new EventPauseAbleRegister(this));
    }
    public void PauseAbleUnRegister()//注销Update
    {
        EventManager.Instance.SendEventImmediately(new EventPauseAbleUnRegister(this));
    }
}