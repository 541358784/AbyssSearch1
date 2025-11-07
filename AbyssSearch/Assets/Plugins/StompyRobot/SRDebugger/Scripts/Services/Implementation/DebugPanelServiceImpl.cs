namespace SRDebugger.Services.Implementation
{
    using System;
    using Internal;
    using SRF;
    using SRF.Service;
    using UI;
    using UnityEngine;

    [Service(typeof (IDebugPanelService))]
    public class DebugPanelServiceImpl : ScriptableObject, IDebugPanelService
    {
        private DebugPanelRoot _debugPanelRootObject;
        public event Action<IDebugPanelService, bool> VisibilityChanged;

        private bool _isVisible;

        private bool? _cursorWasVisible;

        private CursorLockMode? _cursorLockMode;


        public DebugPanelRoot RootObject
        {
            get { return _debugPanelRootObject; }
        }

        public bool IsLoaded
        {
            get { return _debugPanelRootObject != null; }
        }

        public bool IsVisible
        {
            get { return IsLoaded && _isVisible; }
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                if (value)
                {
                    if (!IsLoaded)
                    {
                        Load();
                    }

                    SRDebuggerUtil.EnsureEventSystemExists();

                    _debugPanelRootObject.CanvasGroup.alpha = 1.0f;
                    _debugPanelRootObject.CanvasGroup.interactable = true;
                    _debugPanelRootObject.CanvasGroup.blocksRaycasts = true;
                    _cursorWasVisible = Cursor.visible;
                    _cursorLockMode = Cursor.lockState;

                    if (Settings.Instance.AutomaticallyShowCursor)
                    {
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                }
                else
                {
                    if (IsLoaded)
                    {
                        _debugPanelRootObject.CanvasGroup.alpha = 0.0f;
                        _debugPanelRootObject.CanvasGroup.interactable = false;
                        _debugPanelRootObject.CanvasGroup.blocksRaycasts = false;
                    }

                    if (_cursorWasVisible.HasValue)
                    {
                        Cursor.visible = _cursorWasVisible.Value;
                        _cursorWasVisible = null;
                    }

                    if (_cursorLockMode.HasValue)
                    {
                        Cursor.lockState = _cursorLockMode.Value;
                        _cursorLockMode = null;
                    }
                }

                _isVisible = value;

                if (VisibilityChanged != null)
                {
                    VisibilityChanged(this, _isVisible);
                }
            }
        }

        public DefaultTabs? ActiveTab
        {
            get
            {
                if (_debugPanelRootObject == null)
                {
                    return null;
                }

                return _debugPanelRootObject.TabController.ActiveTab;
            }
        }

        public void OpenTab(DefaultTabs tab)
        {
            if (!IsVisible)
            {
                IsVisible = true;
            }

            _debugPanelRootObject.TabController.OpenTab(tab);
        }

        public void Unload()
        {
            if (_debugPanelRootObject == null)
            {
                return;
            }

            IsVisible = false;

            _debugPanelRootObject.CachedGameObject.SetActive(false);
            Destroy(_debugPanelRootObject.CachedGameObject);

            _debugPanelRootObject = null;
        }

        private void Load()
        {
            var prefab = Resources.Load<DebugPanelRoot>(SRDebugPaths.DebugPanelPrefabPath);

            if (prefab == null)
            {
                Debug.LogError("[SRDebugger] Error loading debug panel prefab");
                return;
            }

            _debugPanelRootObject = SRInstantiate.Instantiate(prefab);
            _debugPanelRootObject.name = "Panel";

            DontDestroyOnLoad(_debugPanelRootObject);

            _debugPanelRootObject.CachedTransform.SetParent(Hierarchy.Get("SRDebugger"), true);
            
            NotchAdapt(_debugPanelRootObject.CanvasGroup.transform.Find("SR_Content") as RectTransform);
            
            SRDebuggerUtil.EnsureEventSystemExists();
        }
        
        private void NotchAdapt(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            int safeAreaOffset = SafeAreaOffset();
            if (safeAreaOffset == 0)
                return;

            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y-safeAreaOffset);
        }
    

        private int SafeAreaOffset()
        {
            int safeAreaOffset = (int) (Screen.height - Screen.safeArea.yMax);
            if (safeAreaOffset == 0)
                return 0;

            safeAreaOffset = safeAreaOffset / 2;
            float scaleRatio = 1.0f * 1366 / Screen.height;
            safeAreaOffset = (int) (safeAreaOffset * scaleRatio);
            safeAreaOffset += 15;

            return safeAreaOffset;
        }
    }
}
