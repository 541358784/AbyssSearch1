using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class ResourcesManager:Singleton<ResourcesManager>
{
    public Dictionary<string, string> PathToPackageNameDic = new Dictionary<string, string>()
    {
        {"Asset","DefaultPackage"}
    };
    public T LoadResource<T>(string name) where T : Object
    {
        var package = GetPackage(name);
        if (package == null)
        {
            Debug.LogError("加载"+name+"失败，资源包为空");
            return null;
        }
        package.LoadAssetSync<T>(name);
        return null;
    }

    public ResourcePackage GetPackage(string name)
    {
        foreach (var pair in PathToPackageNameDic)
        {
            if (name.StartsWith(pair.Key))
                return YooAssets.GetPackage(pair.Value);
        }
        Debug.LogError("路径"+name+"未找到对应包名");
        return null;
    }
    
    
    private IEnumerator InitPackage()
    {
        YooAssets.Initialize();
// 获取指定的资源包，如果没有找到会报错
        var package = YooAssets.GetPackage("DefaultPackage");
// 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
        
        var fileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
    
        var createParameters = new OfflinePlayModeParameters();
        createParameters.BuildinFileSystemParameters = fileSystemParams;
    
        var initOperation = package.InitializeAsync(createParameters);
        yield return initOperation;
    
        if(initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else 
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
    }
}