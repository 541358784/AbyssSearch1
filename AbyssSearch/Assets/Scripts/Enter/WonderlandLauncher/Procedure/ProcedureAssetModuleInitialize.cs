// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:57
// Ver : 1.0.0
// Description : ProcedureAssetModuleInitialize.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using UnityEngine;
using Wonderland.Utility;
using YooAsset;

namespace Wonderland.Launcher
{
    public class ProcedureAssetModuleInitialize : Procedure
    {
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            InitializationOperation initOp = null;

            // 初始化BetterStreaming
            BetterStreamingAssets.Initialize();

            // 初始化资源系统
            YooAssets.Initialize();

            // 创建默认的资源包
            var defaultPackage = YooAssets.CreatePackage(GlobalSetting.DefaultPackageName);
            YooAssets.SetDefaultPackage(defaultPackage);

            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(GlobalSetting.DefaultPackageName);    
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = editorFileSystemParams;
                initOp = defaultPackage.InitializeAsync(createParameters);
            }
            // 联机运行模式
            else
            {
                IRemoteServices remoteServices = new RemoteServices(GlobalSetting.GetHostResUrl());
                IDecryptionServices decryptionServices = new BundleUnpackServices();
                var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, decryptionServices);
                cacheFileSystemParams.AddParameter(FileSystemParametersDefine.INSTALL_CLEAR_MODE, EOverwriteInstallClearMode.None);
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);
                
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = buildinFileSystemParams; 
                createParameters.CacheFileSystemParameters = cacheFileSystemParams;

                initOp = defaultPackage.InitializeAsync(createParameters);
            }

            await initOp.Task;

            if (initOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("LFsmStateInitializeResources:Error:" + initOp.Error);
            }
            else
            {
                Debug.LogError("ProcedureAssetModuleInitialize Succeed!!!!!!");
            }

            return true;
        }
    }
}