using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public abstract class UIWindowController : UIWindow
{
    public GameObject GetItem(string key, GameObject parObj = null)
    {
        if (parObj == null)
        {
            parObj = this.gameObject;
        }

        var obj = FindObj(key, parObj);
        if (obj == null)
        {
            Debug.LogError($"GetItem failed, window controller name : {GetType()},  key = {key}");
        }

        return obj;
    }

    public bool TryGetItem(string key, out GameObject go, GameObject parObj = null)
    {
        if (parObj == null)
        {
            parObj = this.gameObject;
        }

        var trans = parObj.transform.Find(key);
        go = trans ? trans.gameObject : null;
        return go != null;
    }

    public T GetItem<T>(string key, GameObject parObj = null)
    {
        var go = GetItem(key, parObj);
        return GetItem<T>(go);
    }

    public T GetItem<T>(GameObject go)
    {
        if (go != null)
        {
            var com = go.GetComponent<T>();
            if (com == null)
            {
                Debug.LogError(
                    $"GetItem failed, window controller name : {GetType()},  game object name = {go.name}, Component type:{typeof(T)}");
            }

            return com;
        }

        return default(T);
    }
}