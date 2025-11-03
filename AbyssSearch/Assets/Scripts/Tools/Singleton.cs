public class Singleton<T> where T : new()
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            return _instance;
        }
    }
}