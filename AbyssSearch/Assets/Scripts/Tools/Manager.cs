using Framework;
using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>
public class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object m_Lock = new object();
    private static T m_Instance;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    m_Instance = (T)FindObjectOfType(typeof(T));
                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = GameObjectFactory.Create(true, typeof(T) + " (Singleton)");
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T) + " (Singleton)";
                        if (Application.isPlaying) DontDestroyOnLoad(singletonObject);
                        (m_Instance as Manager<T>).InitImmediately();
                    }
                }
                return m_Instance;
            }
        }
    }

    // 这个方法如果override，会在Instance创建完立刻调用, 派生类可以用来默认初始化一些东西
    protected virtual void InitImmediately()
    {
    }
}