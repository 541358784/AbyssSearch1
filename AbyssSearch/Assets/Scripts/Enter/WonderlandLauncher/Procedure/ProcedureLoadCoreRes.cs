// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:57
// Ver : 1.0.0
// Description : ProcedureLoadCoreRes.cs
// ChangeLog :
// **********************************************

using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace Wonderland.Launcher
{
    public class ProcedureLoadCoreRes:Procedure
    {
        private static class AssemblyDefine
        {
            public static List<string> HotRes = new List<string>() {"Assembly-CSharp.dll"};

            public static List<string> AOTRes = new List<string>()
            {
                "DOTween.dll",
                "DragonPlus.Config.Hub.dll",
                "DragonPlus.Core.dll",
                "DragonPlus.Network.dll",
                "DragonPlus.Save.dll",
                "FlatBuffer.dll",
                "Google.Protobuf.dll",
                "Newtonsoft.Json.dll",
                "StrompyRobot.dll",
                "System.Core.dll",
                "System.dll",
                "UniTask.dll",
                "UnityEngine.AndroidJNIModule.dll",
                "UnityEngine.CoreModule.dll",
                "UnityEngine.SpriteAtlasModule.dll",
                "UnityEngine.JSONSerializeModule.dll",
                "YooAsset.dll",
                "mscorlib.dll",
                "spine-unity.dll",
                "EasyTextEffects.dll"
            };
        }
        
        private List<string> _loadedFailedRes = new List<string>();
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            _loadedFailedRes.Clear();
            
            LoadMetaData();

            return await LoadAssemblyData();
        }

        public void LoadMetaData()
        {
            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
                return;
            
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            
            if(AssemblyDefine.AOTRes.Count <= 0)
                return;

            foreach (string aotDllName in AssemblyDefine.AOTRes)
            {
                var textAsset = Resources.Load<TextAsset>("AOTMeta/" + aotDllName);
                if (textAsset != null)
                {
                    var err = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes,
                        HomologousImageMode.SuperSet);
                    Debug.LogError($"LoadMetadata:{aotDllName}. mode:{HomologousImageMode.SuperSet} ret:{err}");
                }
            }
        }

        public async UniTask<bool> LoadAssemblyData()
        {
            if (GlobalSetting.PlayMode == EPlayMode.EditorSimulateMode)
                return true;

            foreach (var hotRes in AssemblyDefine.HotRes)
            {
                if (_loadedFailedRes.Count == 0 || _loadedFailedRes.Contains(hotRes))
                {
                    var assetOperationHandle = YooAssets.LoadAssetAsync<TextAsset>(hotRes);

                    await assetOperationHandle.Task;
                    
                    if (assetOperationHandle.IsDone
                        && assetOperationHandle.Status == EOperationStatus.Succeed
                        && assetOperationHandle.AssetObject != null)
                    {
                        if (_loadedFailedRes.Contains(hotRes))
                        {
                            _loadedFailedRes.Remove(hotRes);
                        }

                        Debug.LogError($"LoadAsset:[ {assetOperationHandle.AssetObject.name} ]");
                   
                        var textAsset = (TextAsset) assetOperationHandle.AssetObject;
                        var assembly = Assembly.Load(textAsset.bytes);
            
                        Debug.LogError($"Assembly [ {assembly.GetName().Name} ] loaded");
                    }
                    else
                    {
                        _loadedFailedRes.Add(hotRes);
                    }
                }
            }

            if (_loadedFailedRes.Count > 0)
            {
                await UniTask.NextFrame();
                await LoadAssemblyData();
            }
            
            return true;
        }
    }
}