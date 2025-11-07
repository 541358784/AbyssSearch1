using Cysharp.Threading.Tasks;
using UnityEngine;
using Wonderland.Utility;
using YooAsset;

namespace Wonderland.Launcher
{
    public class ProcedureAssetModuleInitializeOffline : Procedure
    {
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);

            // 创建默认的资源包
            var defaultPackage = YooAssets.GetPackage(GlobalSetting.DefaultPackageName);
            if (defaultPackage != null)
            {
                var destroyAsync = defaultPackage.DestroyAsync();
                await destroyAsync.Task;
                YooAssets.RemovePackage(defaultPackage);
            }
            
            defaultPackage = YooAssets.CreatePackage(GlobalSetting.DefaultPackageName);
            YooAssets.SetDefaultPackage(defaultPackage);
            InitializationOperation initOp = null;

            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(GlobalSetting.DefaultPackageName);    
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = editorFileSystemParams;
                initOp = defaultPackage.InitializeAsync(createParameters);
            }
            // 单机运行模式
            else
            {
                IDecryptionServices decryptionServices = new BundleUnpackServices();
                
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);
                
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = buildinFileSystemParams; 

                initOp = defaultPackage.InitializeAsync(createParameters);
            }

            await initOp.Task;

            if (initOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("ProcedureAssetModuleInitializeOffline:Error:" + initOp.Error);
            }
            else
            {
                Debug.LogError("ProcedureAssetModuleInitializeOffline Succeed!!!!!!");
            }

            var operation1 = defaultPackage.RequestPackageVersionAsync();
            await operation1.Task;
            if (operation1.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("ProcedureAssetModuleInitializeOffline:Error:" + operation1.Error);
            }

            UpdatePackageManifestOperation operation = null;
            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
            {
                operation = defaultPackage.UpdatePackageManifestAsync("Simulate",30);
                await operation.Task;
            }
            else
            {
                // TODO 这里有问题，不能从package里面获取版本号
                operation = defaultPackage.UpdatePackageManifestAsync(defaultPackage.GetPackageVersion(),30);
                await operation.Task;
            }
            
            Debug.LogError($"UpdateManifest Pass");
            if (operation.Status ==  EOperationStatus.Failed)
            {
                Debug.LogError("UpdateManifest:ErrorInfo=" + operation.Error);
                return false;
            }
            
            return true;
        }
    }
}