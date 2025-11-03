using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager : Singleton<UIManager>
{
    private class PopUpData
    {
        public WindowInfo _info;
        public string _uiPath;
        public object[] _params;
    }
    
    public int extraSiblingIndex = 0;
    private GameObject _uiMaskObj = null;
    
    private Dictionary<UIWindowLayer, List<UIWindow>> _windows = new Dictionary<UIWindowLayer, List<UIWindow>>();
    private Stack<PopUpData> _popUpData = new Stack<PopUpData>();
    
    private bool _havePopUp = false;

    public bool HavePopUpWindow
    {
        get { return _havePopUp; }
    }

    public void Init()
    {
        allWindowMeta();
    }

    public UIWindow OpenUI(string uiPath, params object[] datas)
    {
        if (!_windowsInfo.ContainsKey(uiPath))
        {
            Debug.LogError($"Create ui fail: {uiPath}, no ui info inited");
            return null;
        }
        
        var info = _windowsInfo[uiPath];
        
        return OpenUI(uiPath, info.windowType, info.windowLayer, info.componentType, info.addUIMask, datas);
    }

    public UIWindow OpenUI(string uiPath, UIWindowType forceType, params object[] datas)
    {
        if (!_windowsInfo.ContainsKey(uiPath))
        {
            Debug.LogError($"Create ui fail: {uiPath}, no ui info inited");
            return null;
        }
        
        var info = _windowsInfo[uiPath];
        
        return OpenUI(uiPath, forceType, info.windowLayer, info.componentType, info.addUIMask, datas);
    }
    
    public UIWindow OpenUI(string uiPath, UIWindowType windowType, UIWindowLayer windowLayer, Type type, bool addMask, params object[] datas)
    {
        //windowType = windowLayer == UIWindowLayer.Normal ? UIWindowType.PopupTip : UIWindowType.Normal;
        //windowType = UIWindowType.PopupTip;
        if (windowType == UIWindowType.PopupTip)
        {
            if (HavaPopData(uiPath))
            {
                Debug.LogWarning("[UI]------已经在弹窗堆栈中 稍后弹出");
                return null;
            }

            if (_havePopUp)
            {
                PopUpData data = new PopUpData();
                data._info.componentType = type;
                data._info.windowType = windowType;
                data._info.windowLayer = windowLayer;
                data._info.addUIMask = addMask;
                data._uiPath = uiPath;
                data._params = datas;
                _popUpData.Push(data);
                Debug.LogWarning("[UI]------已经加入弹窗队列 稍后弹出");
                return null;
            }

            _havePopUp = true;
        }
        
        var openUI = GetOpenUI(windowLayer, uiPath);
        if (openUI == null)
        {
            openUI = CreateUI(uiPath, windowType, windowLayer, type, addMask);
        }
        else
        {
            RemoveOpenUI(windowLayer, openUI);
            AddOpenUI(windowLayer, openUI);
        }

        if (openUI == null)
        {
            Debug.LogWarning("[UI]------开启UI 最终失败 " + uiPath);
            return null;
        }
        openUI.gameObject.SetActive(true);
        UpdateUIOrder();
        openUI.OpenWindow(datas);
        return openUI;
    }

    public UIWindow GetOpenUI(UIWindowLayer windowLayer, string uiPath)
    {
        if (!_windows.ContainsKey(windowLayer))
            return null;
        
        foreach (var uiWindow in _windows[windowLayer])
        {
            if(uiWindow == null)
                continue;

            if (uiWindow.windowPath == uiPath)
                return uiWindow;
        }

        return null;
    }

    private bool AddOpenUI(UIWindowLayer windowLayer, UIWindow window)
    {
        if (window == null)
            return false;

        if (!_windows.ContainsKey(windowLayer))
            _windows.Add(windowLayer, new List<UIWindow>());

        if (_windows[windowLayer].Find(a => a == window) != null)
            return false;
        
        _windows[windowLayer].Add(window);

        return true;
    }

    private bool RemoveOpenUI(UIWindowLayer windowLayer, UIWindow window)
    {
        if (window == null)
            return false;

        if (!_windows.ContainsKey(windowLayer))
            return false;

        _windows[windowLayer].Remove(window);
        return true;
    }
    
    public UIWindow GetOpenUI(string uiPath)
    {
        foreach (var kv in _windows)
        {
            foreach (var uiWindow in kv.Value)
            {
                if(uiWindow == null)
                    continue;

                if (uiWindow.windowPath == uiPath)
                    return uiWindow;
            }
        }

        return null;
    }

    public bool HavaPopData(string uiPath)
    {
        foreach (var popUpData in _popUpData)
        {
            if (popUpData._uiPath == uiPath)
                return true;
        }

        return false;
    }

    private UIWindow CreateUI(string uiPath, UIWindowType windowType, UIWindowLayer windowLayer, Type type, bool addMask)
    {
        var window = UIRoot.Instance.CreateWindow(uiPath, type);
        if (window != null)
        {
            window.windowPath = uiPath;
            window.windowType = windowType;
            window.windowLayer = windowLayer;

            InitUIMask(window, addMask);

            AddOpenUI(windowLayer, window);
        }
        
        if(window == null)
            Debug.LogWarning("[UI]------创建弹窗失败 "+ uiPath);
            
        return window;
    }
    
    public void UpdateUIOrder()
    {
        int siblingIndex = 5 + extraSiblingIndex;
        int siblingStep = 10;
        for (var i = 0; i <= (int)UIWindowLayer.Max; i++)
        {
            UIWindowLayer layer = (UIWindowLayer)i;
            if(!_windows.ContainsKey(layer))
                continue;

            var windows = _windows[layer];
            if(windows == null || windows.Count == 0)
                continue;

            for (var j = 0; j < windows.Count; j++)
            {
                if (windows[j] == null)
                {
                    windows.RemoveAt(j);
                    j--;
                    continue;
                }
                
                if(windows[j].canvas == null)
                    windows[j].InitCanvas();
                
                if(windows[j].canvas == null)
                    continue;
                
                if (windows[j].canvas.sortingOrder < 1000)
                {
                    windows[j].canvas.sortingOrder = siblingIndex;
                    windows[j].UpdateCanvasSortOrder();
                }

                siblingIndex += siblingStep;
            }
        }
    }
    
    public bool CloseUI(string uiPath, bool destroy = false)
    {
        var ui = GetOpenUI(uiPath);
        if (ui == null)
            return false;
        
        ui.CloseWindow(destroy);
        if (destroy)
            RemoveOpenUI(ui.windowLayer, ui);

        if (ui.windowType == UIWindowType.PopupTip)
            _havePopUp = false;
        
        PopUpUI();
        return true;
    }

    private UIWindow PopUpUI()
    {
        if (_popUpData == null || _popUpData.Count == 0)
            return null;

        var data = _popUpData.Pop();

        Debug.LogWarning("[UI]------缓存弹窗打开 "+ data._uiPath);
        return OpenUI(data._uiPath, data._info.windowType, data._info.windowLayer, data._info.componentType, data._info.addUIMask, data._params);
    }
    
    public void ClearAllUI()
    {
        foreach (var kv in _windows)
        {
            foreach (var uiWindow in kv.Value)
            {
                if(uiWindow == null)
                    continue;
                
                GameObject.Destroy(uiWindow.gameObject);
            }
        }

        _windows.Clear();
        _popUpData.Clear();
    }
    
    private void InitUIMask(UIWindow uiWindow, bool addUiMask)
    {
        if (uiWindow == null)
            return;

        if (!addUiMask)
            return;

        if (_uiMaskObj == null)
            // _uiMaskObj = ResourcesManager.Instance.LoadResource<GameObject>("Prefabs/Common/UIMask");

        if (_uiMaskObj == null)
            return;

        // GameObject maskObj = Instantiate(_uiMaskObj, uiWindow.transform, false);
        // if (maskObj == null)
        //     return;
        // maskObj.transform.SetAsFirstSibling();

        // Button emptyBut = maskObj.GetComponent<Button>();
        // if (emptyBut == null)
        //     return;
        //
        // emptyBut.onClick.AddListener(uiWindow.ClickUIMask);
    }

    public T GetOpenedUIByPath<T>(string uiPath) where T : UIWindow
    {
        var ui = GetOpenUI(uiPath);
        if(ui == null)
            return null;
        
        return ui.gameObject.activeSelf ? (T)ui : null;
    }

    public T GetOpenUI<T>() where T : UIWindow
    {
        foreach (var kv in _windows)
        {
            foreach (var uiWindow in kv.Value)
            {
                if(uiWindow == null)
                    continue;

                if (uiWindow is T)
                    return (T)uiWindow;
            }
        }

        return null;
    }

    public UIWindow GetOpenedUIByPath(string uiPath)
    {
        var ui = GetOpenUI(uiPath);
        if(ui == null)
            return null;
        
        return ui.gameObject.activeSelf ? ui : null;
    }
    
    public int GetMaxSortingOrder()
    {
        int maxOrder = -1;
        
        for (var i = 0; i <= (int)UIWindowLayer.Max; i++)
        {
            UIWindowLayer layer = (UIWindowLayer)i;
            if(!_windows.ContainsKey(layer))
                continue;

            var windows = _windows[layer];
            if(windows == null || windows.Count == 0)
                continue;

            for (var j = 0; j < windows.Count; j++)
            {
                if (windows[j] == null)
                    continue;
                if(windows[j].canvas == null)
                    continue;
                
                if(!windows[j].gameObject.activeSelf)
                    continue;
                
                maxOrder = Math.Max(maxOrder, windows[j].canvas.sortingOrder);
            }
        }
        
        return maxOrder;
    }

    public int GetOpenUICount()
    {
        int count = 0;
        
        for (var i = 0; i <= (int)UIWindowLayer.Max; i++)
        {
            UIWindowLayer layer = (UIWindowLayer)i;
            if(!_windows.ContainsKey(layer))
                continue;

            var windows = _windows[layer];
            if(windows == null || windows.Count == 0)
                continue;

            for (var j = 0; j < windows.Count; j++)
            {
                if (windows[j] == null)
                    continue;
                if(windows[j].canvas == null)
                    continue;
                if(!windows[j].gameObject.activeSelf)
                    continue;
                count++;
            }
        }

        return count;
    }

    public int LayerMask(UIWindowLayer layer)
    {
        return 1 << ((int) layer);
    }

    public void EnableGraphicRaycaster(bool enable)
    {
        for (var i = 0; i <= (int)UIWindowLayer.Max; i++)
        {
            UIWindowLayer layer = (UIWindowLayer)i;
            if (!_windows.ContainsKey(layer))
                continue;

            var windows = _windows[layer];
            if (windows == null || windows.Count == 0)
                continue;

            for (var j = 0; j < windows.Count; j++)
            {
                if (windows[j] == null)
                    continue;

                if (windows[j].canvas == null)
                    continue;

                var raycaster = windows[j].GetComponent<GraphicRaycaster>();
                if(raycaster == null)
                    continue;
                
                raycaster.enabled = enable;
            }
        }
    }
    
    public void SetCanvasGroupAlpha(bool isShow, bool anim)
    {
        float alpha = isShow ? 1.0f : 0f;
        
        for (var i = 0; i <= (int)UIWindowLayer.Max; i++)
        {
            UIWindowLayer layer = (UIWindowLayer)i;
            if (!_windows.ContainsKey(layer))
                continue;

            var windows = _windows[layer];
            if (windows == null || windows.Count == 0)
                continue;

            for (var j = 0; j < windows.Count; j++)
            {
                if (windows[j] == null)
                    continue;

                if (windows[j].canvasGroup == null)
                    windows[j].canvasGroup = windows[j].gameObject.GetOrCreateComponent<CanvasGroup>();
                
                if (windows[j].canvasGroup == null)
                    continue;
                windows[j].canvasGroup.DOKill();
                if (anim)
                {
                    windows[j].canvasGroup.DOFade(alpha, 0.5f).OnComplete(() =>
                    { });
                }
                else
                {
                    windows[j].canvasGroup.alpha = alpha;
                }
            }
        }
    }
}