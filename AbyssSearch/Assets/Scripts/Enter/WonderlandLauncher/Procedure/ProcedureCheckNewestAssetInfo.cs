// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:58
// Ver : 1.0.0
// Description : ProcedureCheckNewestAssetInfo.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Wonderland.Launcher
{
    public class ProcedureCheckNewestAssetInfo:Procedure
    {
        private string FirstResourceVersionKey = "FirstResourceVersionKey";

        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            return await UpdateManifest();
        }

        protected async UniTask<bool> UpdateManifest()
        {
            Debug.LogError("CheckNewestResInfo");
            
            var defaultPackage = YooAssets.GetPackage(GlobalSetting.DefaultPackageName);
            
#if DEVELOPMENT_BUILD
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                var cacheFilesOperation = defaultPackage.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
                await cacheFilesOperation.Task;
            }
#endif
            var packageVersion = GlobalSetting.ResourceVersion;
            // 1. 获取资源清单的版本信息
            var operation1 = defaultPackage.RequestPackageVersionAsync();
            await operation1.Task;
            if (operation1.Status != EOperationStatus.Succeed)
            {
                string strClientResVersion = PlayerPrefs.GetString(FirstResourceVersionKey);
                if (string.IsNullOrEmpty(strClientResVersion))
                {
                    var copyBuildinManifestOp = new GetBuildinPackageVersionOperation(GlobalSetting.DefaultPackageName);
                    YooAssets.StartOperation(copyBuildinManifestOp);
                    await copyBuildinManifestOp.Task;
                    if (copyBuildinManifestOp.Status == EOperationStatus.Succeed)
                    {
                        PlayerPrefs.SetString(FirstResourceVersionKey, copyBuildinManifestOp.PackageVersion);
                        Debug.LogError(
                            $"----------ProcedureUpdateVersion Success\n使用底包资源版本：{copyBuildinManifestOp.PackageVersion}");
                        packageVersion = copyBuildinManifestOp.PackageVersion;
                    }
                    else
                    {
                        Debug.LogError($"----------ProcedureUpdateVersion failed");
                    }
                }
                // 如果不为空，走本地缓存
            }

            Debug.LogError($"CheckServerVersion Pass");
            var hostServer = GlobalSetting.GetHostResUrl();
          
            Debug.LogError($"GameAssetHost-Server:{hostServer}");

            UpdatePackageManifestOperation operation = null;
            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
            {
                operation = defaultPackage.UpdatePackageManifestAsync("Simulate",30);
                await operation.Task;
            }
            else
            {
                operation = defaultPackage.UpdatePackageManifestAsync(packageVersion,30);
                await operation.Task;
            }
            
 
            Debug.LogError($"UpdateManifest Pass");
            if (operation.Status ==  EOperationStatus.Failed)
            {
                Debug.LogError("UpdateManifest:ErrorInfo=" + operation.Error);
                return false;
            }

            if (operation.Status == EOperationStatus.Succeed)
            {
                packageVersion = YooAssets.GetPackage(GlobalSetting.DefaultPackageName).GetPackageVersion();
                if (!string.IsNullOrEmpty(packageVersion))
                {
                    PlayerPrefs.SetString("ResourceVersionKey", packageVersion);
                    PlayerPrefs.SetString(FirstResourceVersionKey, packageVersion);
                    Debug.LogError("Store Local Res Version:" + packageVersion);
                }
            }

            return true;
        }
    }
}