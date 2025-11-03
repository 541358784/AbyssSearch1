using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// gameobject对象工厂，封装了gameobject的创建和销毁
    /// </summary>
    public static class GameObjectFactory
    {
        private static Transform _defaultRoot;


        public static GameObject Create(bool dontDestroyOnLoad)
        {
            try
            {
                var go = new GameObject();
                if (dontDestroyOnLoad) GameObject.DontDestroyOnLoad(go);
                go.transform.parent = GetDefaultRoot();
                return go;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return null;
        }

        public static void Destroy(Object go)
        {
            try
            {
                Object.Destroy(go);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static GameObject Clone(GameObject templateGo, GameObject parObj = null)
        {
            try
            {
                if (templateGo != null)
                {
                    Transform parent = null;
                    if (parObj != null && parObj.transform != null)
                    {
                        parent = parObj.transform;
                    }

                    if (parent == null)
                    {
                        parent = templateGo.transform.parent;
                    }

                    if (parent == null)
                    {
                        return Object.Instantiate(templateGo);
                    }

                    return Object.Instantiate(templateGo, parent, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return null;
        }

        public static Transform GetDefaultRoot()
        {
            try
            {
                if (_defaultRoot == null)
                {
                    GameObject gameObj = new GameObject("GameObjectFactoryRoot");
                    Object.DontDestroyOnLoad(gameObj);
                    _defaultRoot = gameObj.transform;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return _defaultRoot;
        }
    }
}