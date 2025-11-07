// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/16:28
// Ver : 1.0.0
// Description : MatchPuzzle3DProcedureController.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using Wonderland.Utility;
using TMPro;
using UnityEngine.UI;

namespace Wonderland.Launcher
{
    public class WonderlandProcedureController
    {
        private Procedure[] Procedures;
        private Procedure[] OfflineProcedures;
        
        public float displayProgress = 0.0f;
        public bool procedureFinished = false;
        
        public async void StartInitializeProcedures(ProcedureContext procedureContext)
        {
            Procedures = new Procedure[]
            {
                new ProcedureAdaptScreenView(),
                new ProcedureSplashView(),
                new ProcedureCheckNewestAssetVersion(),
                new ProcedureAssetModuleInitialize(),
                new ProcedureCheckNewestAssetInfo(),
                new ProcedureShowGameLoading(),
                new ProcedureLoadCoreRes(),
                new ProcedureStartGamePlay()
            };
            OfflineProcedures = new Procedure[]
            {
                new ProcedureAssetModuleInitializeOffline(),
                new ProcedureShowGameLoading(),
                new ProcedureLoadCoreRes(),
                new ProcedureStartGamePlay()
            };

            procedureFinished = false;
            
            UpdateLoadingProcess(procedureContext);

            bool useOffline = false;
            for (var i = 0; i < Procedures.Length; i++)
            {
                bool toNext = await Procedures[i].ExecuteProcedure(procedureContext);
                if (!toNext)
                {
                    useOffline = true;
                    break;
                }
            }

            if (useOffline)
            {
                for (var i = 0; i < OfflineProcedures.Length; i++)
                {
                    bool toNext = await OfflineProcedures[i].ExecuteProcedure(procedureContext);
                    if (!toNext)
                    {
                        break;
                    }
                }
            }
            procedureFinished = true;
        }

        public async void UpdateLoadingProcess(ProcedureContext procedureContext)
        {
            while (procedureContext.DebugText == null)
                await UniTask.NextFrame();

            while (!procedureFinished)
            {
                
#if DEVELOPMENT_BUILD          
                procedureContext.DebugText.SetText(procedureContext.ProcedureName);
#endif
                
                if (displayProgress < procedureContext.currentLoadingProcess)
                {
                    displayProgress = displayProgress + (procedureContext.currentLoadingProcess - displayProgress) * 0.1f;
                }
            
                ShowInitializeText(procedureContext, displayProgress);
                
                await UniTask.NextFrame();
            }
        }
        
        public void ShowInitializeText(ProcedureContext procedureContext, float initializeProgress)
        {
            if (procedureContext.LoadingView != null)
            {
                var slider = procedureContext.LoadingView.transform.Find("ProgressSlider").GetComponent<Slider>();
                var textMeshProUGUI = procedureContext.LoadingView.transform.Find("ProgressSlider/ProgressText").GetComponent<TextMeshProUGUI>();
                slider.value = initializeProgress;
                textMeshProUGUI.text = LocaleHelper.GetInitializeText(initializeProgress * 100);
            }
        }
    }
}