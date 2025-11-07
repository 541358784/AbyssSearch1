// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/16:17
// Ver : 1.0.0
// Description : ProcedureSplashView.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using Wonderland.Utility;
using UnityEngine;

namespace Wonderland.Launcher
{
    public class ProcedureSplashView : Procedure
    {
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            var uiLoading = Resources.Load<GameObject>("UILoading/Prefabs/UILoading");
            var uiCanvas = GameObject.Find("Root/UIRoot/UICanvas");
 
            var soundClose = PlayerPrefs.GetInt("Wonderland_Setting_SoundClose", 0);
          
            if (soundClose > 0)
            {
                var audioSource = uiLoading.GetComponent<AudioSource>();
                if (audioSource != null)
                    audioSource.enabled = false;
            }
            
            if (uiCanvas != null)
            {
                var loadView = GameObject.Instantiate(uiLoading, uiCanvas.transform);
                loadView.name = "UILoading";
                LocaleHelper.MatchLocale();
            }
            
            
            
                        
#if DEBUG || DEVELOPMENT_BUILD
            var rootGameObject = GameObject.Find("Root");
            if(!rootGameObject.GetComponent<ApplicationExceptionHandler>())
                rootGameObject.AddComponent<ApplicationExceptionHandler>();
#endif
            return true;
        }
    }
}