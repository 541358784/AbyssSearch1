using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public abstract class UIWindowData
{
}

public abstract class UIWindow : MonoBehaviour
{
    public Canvas canvas = null;
    public CanvasGroup canvasGroup = null;
    //public Canvas[] childCanvas = null;
    
    public Action UIcloseAction;
    private string _mWindowPath;

    // 是否播放默认对话框的声音
    public bool isPlayDefaultAudio = true;

    protected Animator _animator;
    public string windowPath
    {
        set { _mWindowPath = value; }
        get { return _mWindowPath; }
    }

    [FormerlySerializedAs("m_WindowType")] [HideInInspector] public UIWindowType windowType;

    //[HideInInspector]
    [FormerlySerializedAs("uiWindowLayer")] public UIWindowLayer windowLayer;

    [HideInInspector] public bool mIsOpen = false;

    public bool IsWindowOpened
    {
        get { return mIsOpen; }
    }

    protected bool canClickMask = false;

    void Awake()
    {
        InitCanvas();

        _animator = transform.GetComponent<Animator>();
        
        gameObject.GetOrCreateComponent<GraphicRaycaster>();

        PrivateAwake();
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(XUtility.WaitSecondsIEnumerator(0.5f, () => { canClickMask = true; }));
    }

    public void InitCanvas()
    {
        if (canvas != null)
            return;

        canvas = gameObject.GetOrCreateComponent<Canvas>();
        canvas.overrideSorting = true;
    }
    // void Start()
    // {
    // }

    public void OpenWindow(params object[] objs)
    {
        ShowUI();
        OnOpenWindow(objs);
        OpenWindowAudio();
    }

    protected virtual void OpenWindowAudio()
    {
        if (isPlayDefaultAudio)
        {
            
        }
    }

    void CloseWindowAudio()
    {
        if (isPlayDefaultAudio && mIsOpen)
        {
            
        }
    }

    public void CloseWindow(bool destroy = false)
    {
        OnCloseWindow(destroy);
        CloseWindowAudio();
        //Global.hideZhezhao();

        if (destroy)
        {
            DestroyUI();
        }
        else
        {
            HideUI();
        }

    }

    public virtual bool OnBack()
    {
        return UIManager.Instance.CloseUI(_mWindowPath, true);
    }

    // public void CloseWindowWithinUIMgr(bool destroy = false)
    // {
    //     UIManager.Instance.CloseUI(_mWindowPath, destroy);
    // }
    public void CloseWindowWithinUIMgr(bool destroy = false, Action afterCloseFunc = null, Action beforeCloseFunc = null) 
    {
        beforeCloseFunc?.Invoke();
        UIManager.Instance.CloseUI(_mWindowPath, destroy);
        afterCloseFunc?.Invoke();
    }

    public void ReloadWindow()
    {
        ShowUI();
        OnReloadWindow();
    }

    private void ShowUI()
    {
        mIsOpen = true;
        if (gameObject == null)
        {
            Debug.Log("UI已被销毁:" + windowPath);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void HideUI()
    {
        mIsOpen = false;
        gameObject.SetActive(false);
    }

    private void DestroyUI()
    {
        mIsOpen = false;
        Destroy(gameObject);
    }

    #region 子类继承重写

    /// <summary>
    /// 打开界面时调用
    /// </summary>
    /// <param name="objs"></param>
    protected virtual void OnOpenWindow(params object[] objs)
    {
    }

    /// <summary>
    /// 关闭界面时调用
    /// </summary>
    protected virtual void OnCloseWindow(bool destroy = false)
    {
    }

    /// <summary>
    /// 重新加载界面时调用
    /// </summary>
    protected virtual void OnReloadWindow()
    {
    }

    /// <summary>
    /// 私有Awake方法，会在基类Awake执行后调用
    /// </summary>
    public abstract void PrivateAwake();

    public GameObject BindEvent(string target, GameObject par = null, Action<GameObject> action = null,
        bool playAudio = true)
    {
        if (par == null)
        {
            par = this.gameObject;
        }

        GameObject obj = par?.transform.Find(target)?.gameObject;
        if (obj != null)
        {
            Button button = obj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener
                (
                    delegate()
                    {
                        if (playAudio)
                        {
                            
                        }

                        action?.Invoke(obj);
                    }
                );
            }
        }
        else
        {
            Debug.LogError("未找到　" + gameObject.name + "/" + target);
        }

        return obj;
    }

    public GameObject FindObj(string path, GameObject par = null)
    {
        if (par == null)
        {
            par = this.gameObject;
        }

        GameObject obj = par.transform.Find(path)?.gameObject;
        return obj;
    }

    #endregion

    public virtual void ClickUIMask()
    {
        if (!canClickMask)
            return;

        canClickMask = false;
        AnimCloseWindow();
    }

    public void SetCanClickMask(bool canClick)
    {
        canClickMask = canClick;
    }
    
    public virtual void AnimCloseWindow(Action afterCloseFunc = null, bool destroy = true, Action beforeCloseFunc = null)
    {
        if (_animator == null)
        {
            CloseWindowWithinUIMgr(destroy, afterCloseFunc, beforeCloseFunc);
            return;
        }

        _animator.PlayAnimation(UIAnimationConst.DisAppear, () =>
        {
            CloseWindowWithinUIMgr(destroy, afterCloseFunc, beforeCloseFunc);
        });
    }
    
    public virtual void UpdateCanvasSortOrder()
    {
    }
}