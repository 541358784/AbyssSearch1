// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:58
// Ver : 1.0.0
// Description : ProcedureCheckNewestAssetVersion.cs
// ChangeLog :
// **********************************************

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Wonderland.Launcher
{
    public class ProcedureCheckNewestAssetVersion:Procedure
    {
        private UniTaskCompletionSource _uniTaskCompletionSource;
        
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            string strClientResVersion = PlayerPrefs.GetString("ResourceVersionKey", GlobalSetting.ResourceVersion);

            try
            {
                //避免覆盖安装，读取本地版本配置，导致资源版本不对的问题
                var lastLocalVersion = int.Parse(strClientResVersion.Replace("v", ""));
                var packageDefaultVersion = int.Parse(GlobalSetting.ResourceVersion.Replace("v", ""));

                if (lastLocalVersion > packageDefaultVersion)
                {
                    GlobalSetting.ResourceVersion = strClientResVersion;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {

                await GetAssetVersionInfo("127.0.0.1", "0");
            }
            else
            {
                Debug.LogError($"Network not NotReachable: use lastResourceVersion:{strClientResVersion}");
            }

            return true;
        }

        private void ParseRemoteAssetVersion(string requestAssetVersionInfo)
        {
            if (string.IsNullOrEmpty(requestAssetVersionInfo))
                return;
        }
        
        
        
        protected async UniTask GetAssetVersionInfo(string versionServerUrl, string playerId)
        {
            var url = $@"{versionServerUrl}/version?id={playerId}&code={GlobalSetting.RootVersion}&platform=android";

            Debug.LogError($"==== versionServerUrl url:{url}");

            var request = new UnityWebRequest(new Uri(url), UnityWebRequest.kHttpVerbGET);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeCertificateHandlerOnDispose = true;
            request.timeout = 4;
          
            Debug.LogError($"SendRequest");
           
            request.SendWebRequest();
            var timer = 0f;
           
            while (!request.isDone)
            {
                await UniTask.NextFrame();

                timer += Time.deltaTime;
                
                if (timer >= 4f && !request.isDone)
                {
                    request.Dispose();
                    Debug.LogError($"GetAssetVersionError Time out Failed Not Done");
                    return;
                }
            }

            if (request.isDone)
            {
                Debug.Log($"FetchedVersionDone...");
               
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Success:${request.downloadHandler.text}");
                    ParseRemoteAssetVersion(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"GetAssetVersionError:{request.result.ToString()} / {request.error}");
                }
            }
            else
            {
                request.Dispose();
                Debug.LogError($"GetAssetVersionError Failed Not Done");
            }
        }

    }
}