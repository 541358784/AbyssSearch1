using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class UIRoot : MonoBehaviour
{
    public Canvas mRootCanvas;

    // 所有UI的根节点
    public GameObject mRoot;
    public GameObject mUIRoot;
    public Camera mUICamera;
    public GameObject mEventSystem;
    private EventSystem _eventSystem;

    private CanvasGroup _canvasGroup;
    public GameObject m_TouchBlock;
    public enum ECanvasScaler
    {
        S1365_768,
        S1334_750,
    }

    // 缓存UI摄像机的6个裁剪面
    public Plane[] CameraPlanes { private set; get; }

    // 尝试修复花屏的问题
    bool isDirty = false;
    Font dirtyFont = null;

    
    private Canvas _canvas;

    private static UIRoot m_Instance;

    public static UIRoot Instance
    {
        get { return m_Instance; }
    }

    private void Awake()
    {
        m_Instance = this;

        DontDestroyOnLoad(this.gameObject);
        if (mUICamera != null)
        {
            CameraPlanes = GeometryUtility.CalculateFrustumPlanes(mUICamera);
        }

        Font.textureRebuilt += delegate(Font font1)
        {
            isDirty = true;
            dirtyFont = font1;
        };

        _canvas = mRoot.GetComponent<Canvas>();

        _eventSystem = mEventSystem.GetComponent<EventSystem>();
    }


    private void LateUpdate()
    {
        if (isDirty)
        {
            isDirty = false;
            foreach (Text text in FindObjectsOfType<Text>())
            {
                if (text.font == dirtyFont)
                {
                    text.FontTextureChanged();
                }
            }

            dirtyFont = null;
        }
    }

    /// <summary>
    /// Creates the window.
    /// </summary>
    /// <returns>The window.</returns>
    /// <param name="windowName">UI预设名</param>
    /// <param name="type">UI类型</param>
    /// <param name="layer">UI层级</param>
    public UIWindow CreateWindow(string windowName, Type componentType)
    {
        GameObject uiPrefab = LoadPrefab(windowName);
        if (uiPrefab == null)
        {
            Debug.LogError($"{GetType()}.CreateWindow, cannot find window resource : {windowName}");
            return null;
        }
        UIWindow window = null;
        GameObject obj = Instantiate(uiPrefab, mUIRoot.transform, false) as GameObject;
        window = obj.AddComponent(componentType) as UIWindow;
        return window;
    }

    GameObject LoadObj(string windowName)
    {
        GameObject uiPrefab = null;
        uiPrefab = ResourcesManager.Instance.LoadResource<GameObject>(windowName);
        return uiPrefab;
    }

    public GameObject LoadPrefab(string wName)
    {
        string windowName = wName;
        GameObject uiPrefab = null;
        
        uiPrefab = LoadObj(windowName);
        if (uiPrefab == null) // 增加一层兼容。如果ab包里加载不到，就去Resources目录里加载
        {
            uiPrefab = Resources.Load<GameObject>(Path.Combine("Loading", windowName));
        }

        return uiPrefab;
    }


    public Vector2 GetScreenCanvasScale()
    {
        var rectTransform = mRoot.GetComponent<RectTransform>();
        var screenSize = new Vector2(Screen.width, Screen.height);
        return new Vector2(screenSize.x / rectTransform.rect.width, screenSize.y / rectTransform.rect.height);
    }

    public void EnableTouch(bool b)
    {
        if (_canvasGroup == null)
            _canvasGroup = mRootCanvas.GetComponent<CanvasGroup>();

        _canvasGroup.interactable = b;
        _canvasGroup.blocksRaycasts = b;
    }

    public void SwitchCanvasScaler(ECanvasScaler scaler)
    {
        CanvasScaler cs = mRootCanvas.transform.GetComponent<CanvasScaler>();
        if (cs != null)
        {
            switch (scaler)
            {
                case ECanvasScaler.S1365_768:
                    cs.referenceResolution = new Vector2(1365, 768);
                    break;
                case ECanvasScaler.S1334_750:
                default:
                    cs.referenceResolution = new Vector2(1334, 750);
                    break;
            }
        }
    }

    public Vector2 ScreenToLocal(Vector3 sPos)
    {
        Vector2 outVec;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mRootCanvas.transform as RectTransform, sPos, mUICamera,
            out outVec);

        return outVec;
    }

    public bool EnableEventSystem
    {
        set
        {
            if (mEventSystem == null)
                return;

            mEventSystem.SetActive(value);
            //Debug.LogError("EnableEventSystem="+ value +" 当前调用栈：\n" + System.Environment.StackTrace);
        }
        get
        {
            if (mEventSystem == null)
                return false;

            return mEventSystem.activeSelf;
        }
    }

    public void AddTouchBlock()
    {
        m_TouchBlock.SetActive(true);
    }

    public void RemoveTouchBlock()
    {
        m_TouchBlock.SetActive(false);
    }

    public bool IsInputDisable()
    {
        if (m_TouchBlock == null) return false;
        return m_TouchBlock.activeSelf;
    }
    
    public bool IsPointInArea(Vector2 position, RectTransform rect)
    {
        Rect worldRect = rect.GetWorldRect();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(mRootCanvas.transform as RectTransform, position,
            mUICamera, out Vector3 worldPosition);
        return worldRect.Contains(worldPosition);
    }
}