// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:42
// Ver : 1.0.0
// Description : ProcedureAdaptScreenView.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace Wonderland.Launcher
{
    public class ProcedureShowGameLoading:Procedure
    {
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            Debug.LogError("Begin Load AssetLoading!!!!!!!!!!");
            var assetOperationHandle = YooAssets.LoadAssetAsync<GameObject>("AssetLoading");
            await assetOperationHandle.Task;
            var splashView = GameObject.Find("Root/UIRoot/UICanvas/UILoading");
            var uiCanvas = GameObject.Find("Root/UIRoot/UICanvas");
            var loadView = assetOperationHandle.InstantiateSync(uiCanvas.transform);
            
            procedureContext.LoadingView = loadView;
            ResetLoadView(loadView, procedureContext);
            
            GameObject.DestroyImmediate(splashView);

            return true;
        }
        private void ResetLoadView(GameObject loadView, ProcedureContext procedureContext)
        {
            loadView.name = "UILoading";
            loadView.transform.Find("ProgressSlider").GetComponent<Slider>().value = 0;
            loadView.transform.Find("ProgressSlider/ProgressText").GetComponent<TMP_Text>().text = "";      
            var debugText = loadView.transform.Find("ProgressSlider/LoadingTips").GetComponent<TextMeshProUGUI>();
            
            if(debugText != null)
                procedureContext.DebugText = debugText;
        }
    }
}