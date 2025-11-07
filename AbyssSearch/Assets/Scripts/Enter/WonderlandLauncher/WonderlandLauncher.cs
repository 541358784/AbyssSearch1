// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2023/06/25/10:31
// Ver : 1.0.0
// Description : Launcher.cs
// ChangeLog :
// **********************************************

using System;
using System.Collections.Generic;
using Wonderland;
using Wonderland.Launcher;
using UnityEngine;
using UnityEngine.U2D;

namespace Wonderland.Launcher
{
    public class WonderlandLauncher : MonoBehaviour
    {
#if UNITY_EDITOR
        public static Dictionary<string, Action<SpriteAtlas>> RequestAtlasCallback =
            new Dictionary<string, Action<SpriteAtlas>>();

        private void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        private static void OnAtlasRequested(string spriteAtlasName, Action<SpriteAtlas> callback)
        {
            RequestAtlasCallback.Add(spriteAtlasName, callback);
        }

        public static void RemoveListener()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
        }
#endif
        //SetUpViewResolution
        //1.CheckResVersion
        //2.SelectServer
        //3.ShowSplashView
        //4.CheckServerVersion
        //5.Check & And Update Manifest 
        //6.LauncherGame
        private void Start()
        {
            Debug.Log($"Launcher Start : {Time.time}");
            MatchQuality();

          
            StartLaunchProcedure();
        }

        public async void StartLaunchProcedure()
        {
#if !PRODUCTION_PACKAGE
            var severSelectAsset = Resources.Load<GameObject>("Debug/ServerOption");
            var severSelect =
                GameObject.Instantiate(severSelectAsset, GameObject.Find("Root/UIRoot/UICanvas").transform);
            var serverSelector = severSelect.AddComponent<ResServerSelector>();
            await serverSelector.WaitServerSelection();
#endif
            
            Debug.Log($"Launcher Start After Select : {Time.time}");
            // await SDK.Instance.OnGameLaunch();
            Debug.Log($"Launcher Start loading : {Time.time}");
 
            var controller = new WonderlandProcedureController();
            controller.StartInitializeProcedures(new ProcedureContext());
        }

        public void MatchQuality()
        {
            var currentTier = DeviceQualityEvaluator.CurrentTier;
            Debug.LogError($"DeviceQualityEvaluator.CurrentTier:{currentTier}");

            var useHighQuality = true;
            if (Application.platform == RuntimePlatform.Android) //CK只需对android机型做优化
            {
                if ((SystemInfo.processorFrequency != 0 && SystemInfo.processorFrequency < 1250) || // CPU低于1.2GH
                    (SystemInfo.systemMemorySize != 0 && SystemInfo.systemMemorySize < 1200)) // 或者内存低于1G
                {
                    useHighQuality = false;
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer) //苹果设备不能获取cpu频率，这里通过GPU型号和内存判断
            {
                //https://zh.wikipedia.org/wiki/IOS%E5%92%8CiPadOS%E8%AE%BE%E5%A4%87%E5%88%97%E8%A1%A8
            }

            Application.targetFrameRate = useHighQuality ? 60 : 30;
            
#if UNITY_ANDROID         
            Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
#endif
        }
        
        
    }
}