using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Google.Android.AppBundle.Editor;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;

public class AppBuilder
{
    
    public class FileOffsetEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            return AesEncrypt(fileInfo);
        }
        
        private EncryptResult AesEncrypt(EncryptFileInfo fileInfo)
        {
            byte[] bKey = Encoding.UTF8.GetBytes("aeskey");
            
            Array.Reverse(bKey);
           
            var aes = new RijndaelManaged();;
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.PKCS7;
            
            byte[] encrypt = null;
            byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);

            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bKey),
                    CryptoStreamMode.Write))
                {
                    cStream.Write(fileData, 0, fileData.Length);
                    cStream.FlushFinalBlock();
                    encrypt = mStream.ToArray();
                }
            }

            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = encrypt;
            
            return result;
        }
    }
    
    private static class BuildVersionSetting
    {
#if UNITY_IOS
        public const string bundleFolderName = "ios";
#elif UNITY_ANDROID
        public const string bundleFolderName = "android";
#endif
        public static string serverVersion = "v1";
        public static string resourcesVersion = "v43";
        public static string resourcesRootVersion = "rv11";
        public static string version = "1.0.21";
        public static string versionCode = "22";
    }


    private static class AssemblyDefine
    {
        public static List<string> HotAssembly = new List<string>() {"Assembly-CSharp.dll"};

        public static List<string> AOTAssembly = new List<string>()
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
            "spine-timeline.dll",
            "Unity.Timeline.dll",
            "EasyTextEffects.dll",
        };
    }

    [MenuItem("HybridCLR/Generate/AllExtend")]
    public static void HybridCLR_GenerateAll(bool isDebug)
    {
        SetupDragonSDK(isDebug);
        HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
    }

    [MenuItem("AppBuilder/AssetBundle/Debug")]
    public static void BuildAssetBundle_Debug()
    {
        CompileDLL(true);
        BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget, true);
    }

    [MenuItem("AppBuilder/AssetBundle/Release")]
    public static void BuildAssetBundle_Release()
    {
        CompileDLL(false);
        BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget, false);
    }

    public static void BuildPlayerAssetBundle_Release()
    {
        BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget, false, true);
    }

    // public static void BuildPlayerAssetBundle_Debug()
    // {
    //     BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget, true,true);
    // }

    [MenuItem("AppBuilder/Player/Debug")]
    public static void BuildPlayer_Debug()
    {
        GenerateAll(true);
        BuildPlayer(EditorUserBuildSettings.activeBuildTarget, true, true, false);
    }

    [MenuItem("AppBuilder/Player/Debug_AAB")]
    public static void BuildPlayer_Debug_AAB()
    {
        GenerateAll(true);
        BuildPlayer(EditorUserBuildSettings.activeBuildTarget, true, true, true);
    }

    [MenuItem("AppBuilder/Player/Release")]
    public static void BuildPlayer_Release()
    {
        GenerateAll(false);
        BuildPlayer(EditorUserBuildSettings.activeBuildTarget, false, true, false);
    }

    [MenuItem("AppBuilder/Player/Release_AAB")]
    public static void BuildPlayer_Release_AAB()
    {
        GenerateAll(false);
        BuildPlayer(EditorUserBuildSettings.activeBuildTarget, false, true, true);
    }
    
    private static void SetUpSymbolDefine(bool isDebug)
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        

        string defineSymbolString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(target == BuildTarget.Android
                ? BuildTargetGroup.Android
                : BuildTargetGroup.iOS);
        
        if (!defineSymbolString.EndsWith(";"))
        {
            defineSymbolString += ";";
        }
 
        if (!isDebug && defineSymbolString.IndexOf("PRODUCTION_PACKAGE", StringComparison.Ordinal) < 0)
        {
            defineSymbolString += "PRODUCTION_PACKAGE;";
        }
        else if (isDebug && !defineSymbolString.Contains("DEVELOPMENT_BUILD"))
        {
            defineSymbolString = "DEVELOPMENT_BUILD;" + defineSymbolString;
        }
        
        if (!defineSymbolString.Contains("GOOGLE_UMP_ENABLE"))
        {
            defineSymbolString = "GOOGLE_UMP_ENABLE;" + defineSymbolString;
        }
        
        if (!defineSymbolString.Contains("ENABLE_HYBRIDCLR"))
        {
            defineSymbolString = "ENABLE_HYBRIDCLR;" + defineSymbolString;
        }

        if (!defineSymbolString.Contains("DISABLE_STORAGE_LOG"))
        {
            defineSymbolString = "DISABLE_STORAGE_LOG;" + defineSymbolString;
        }

        if (!defineSymbolString.Contains("ENABLE_LOG") && isDebug)
        {
            defineSymbolString = "ENABLE_LOG;" + defineSymbolString;
        }
        
        if (!isDebug)
        {
            if (defineSymbolString.Contains("ENABLE_LOG"))
            {
                if (defineSymbolString.Contains("ENABLE_LOG;"))
                {
                    defineSymbolString = defineSymbolString.Replace("ENABLE_LOG;", "");
                }
                else
                {
                    defineSymbolString = defineSymbolString.Replace("ENABLE_LOG", "");
                }
            }
            if(!defineSymbolString.Contains("ENABLE_ERROR_AND_ABOVE_LOG"))
                defineSymbolString = "ENABLE_ERROR_AND_ABOVE_LOG;" + defineSymbolString;
        }
           
        if (target == BuildTarget.Android)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbolString);
        }
        else if (target == BuildTarget.iOS)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbolString);
        }

        EditorUserBuildSettings.development = isDebug;
        
        
        WriteBundleVersionToGameModuleCode();
    }

    private static void BuildAssetBundle(BuildTarget target, bool isDebug, bool copyBuildInBundle = false)
    {

        if (target == BuildTarget.iOS)
        {
            EncryptLocaleKv("AssetInApp/Configs/LocaleConfig");

            if (copyBuildInBundle)
                EncryptLocaleKv("Resources/LocaleConfig");
        }

#if UNITY_IOS
        if(!isDebug)
            YooAssetConfigDataBuilder.UpdateIosTMBundleSetting();
#endif        

        //更新GlobalSetting 的 bundleVersion
      
        //YooAsset Build
        
        IBuildPipeline pipeline = null;
        BuildParameters buildParameters = null;
        EBuildPipeline buildPipeline = EBuildPipeline.ScriptableBuildPipeline;
        
        ScriptableBuildParameters scriptableBuildParameters = new ScriptableBuildParameters();
            
        // 执行构建
        pipeline = new ScriptableBuildPipeline();
        buildParameters = scriptableBuildParameters;
            
        scriptableBuildParameters.CompressOption = ECompressOption.LZ4;
        scriptableBuildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName("MatchMatch");
        scriptableBuildParameters.TrackSpriteAtlasDependencies = true;
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = buildPipeline.ToString();
        buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
        buildParameters.PackageName = "MatchMatch";
        buildParameters.PackageVersion = BuildVersionSetting.resourcesVersion;
        buildParameters.VerifyBuildingResult = true;
        // 启用共享资源打包
        buildParameters.EnableSharePackRule = true;
        buildParameters.FileNameStyle = EFileNameStyle.HashName;

        if (copyBuildInBundle)
        {
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyByTags;
        }
        else
        {
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
        }

        buildParameters.BuildinFileCopyParams = "BuildIn";
        
        buildParameters.EncryptionServices = new FileOffsetEncryption();
        buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
        buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！
        if (Directory.Exists(buildParameters.BuildOutputRoot))
            Empty(buildParameters.BuildOutputRoot);

        var buildResult = pipeline.Run(buildParameters, true);
        // if (buildResult.Success)
        // {
        //     Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
        // }
        // else
        // {
        //     Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
        // }
     //    
     //    BuildParameters buildParameters;
     //    var scriptableBuildParameters = new ScriptableBuildParameters();
     //
     //    scriptableBuildParameters.BuildOutputRoot =  AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
     //    scriptableBuildParameters.BuildinFileRoot =  AssetBundleBuilderHelper.GetStreamingAssetsRoot();
     //    
     //    scriptableBuildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
     //    //buildParameters.BuildMode = isDebug ? EBuildMode.IncrementalBuild : EBuildMode.ForceRebuild;
     //    scriptableBuildParameters.BuildMode =  EBuildMode.IncrementalBuild;
     //    scriptableBuildParameters.PackageName = "MatchMatch";
     //    scriptableBuildParameters.PackageVersion = BuildVersionSetting.resourcesVersion;
     //    scriptableBuildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
     //    scriptableBuildParameters.CopyBuildinFileTags = "BuildIn";
     //    scriptableBuildParameters.VerifyBuildingResult = true;
     //    scriptableBuildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
     //    scriptableBuildParameters.CompressOption = ECompressOption.LZ4;
     //    scriptableBuildParameters.OutputNameStyle = EOutputNameStyle.HashName;
     //    scriptableBuildParameters.SharedPackRule = new ZeroRedundancySharedPackRule();
     // //   buildParameters.ShareAssetPackRule = new DefaultShareAssetPackRule();
     //    scriptableBuildParameters.EncryptionServices = new FileOffsetEncryption();
     //    
     //
     //    if (copyBuildInBundle)
     //    {
     //        buildParameters.CopyBuildinFileOption = ECopyBuildinFileOption.ClearAndCopyByTags;
     //    }
     //    else
     //    {
     //        buildParameters.CopyBuildinFileOption = ECopyBuildinFileOption.None;
     //    }
     //
     //    buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
     //    buildParameters.SBPParameters.WriteLinkXML = true;
     //
     //
     //    if (Directory.Exists(buildParameters.BuildOutputRoot))
     //        Empty(buildParameters.BuildOutputRoot);
     //
     //    var builder = new YooAsset.Editor.AssetBundleBuilder();
     //    var buildResult = builder.Run(buildParameters);

        if (!buildResult.Success)
        {
            throw new Exception($"Yoo Assets:Build task error : {buildResult.ErrorInfo}");
        }

        if (Directory.Exists(Application.dataPath + "/../ServerData"))
            Directory.Delete(Application.dataPath + "/../ServerData", true);

        var sourceDirectory = buildResult.OutputPackageDirectory;

        var targetDirectory = Path.Combine(EditorTools.GetProjectPath(),
            "ServerData",
            BuildVersionSetting.bundleFolderName,
            BuildVersionSetting.resourcesRootVersion,
            BuildVersionSetting.resourcesVersion);

        EditorTools.CopyDirectory(sourceDirectory, targetDirectory);

        // PushBundleBuildInfoToGit(isDebug);
        CreateResVersionTxt();
        
        Debug.Log("BuildAssetBundle......Done");
    }

    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName(string packageName)
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(packageName, uniqueBundleName);
    }
    
    [MenuItem("AppBuilder/Player/IOSConfig加密测试")]
    public static void EncryptBuildInLocalConfigFile()
    {
        // EncryptLocaleKv("InstallAssets/Configs/LocaleConfig");
        // EncryptLocaleKv("Resources/LocaleConfig");
    }

    public static void EncryptLocaleKv(string rootFolder)
    {
#if UNITY_IOS
        var allFiles = Directory.GetFiles(Path.Combine(Application.dataPath, rootFolder));

        foreach (var file in allFiles)
        {
            if (!file.EndsWith(".json"))
                continue;
            var textFileName = Path.GetFileName(file);
            textFileName = textFileName.Replace(Path.GetExtension(textFileName), "");

            // if(!textFileName.StartsWith("locale_loading_")) 
            //     continue;
       
            if (!textFileName.Contains("_ios"))
            {
                var configTextAsset = File.ReadAllText(file);

                var listConfig = JsonConvert.DeserializeObject<List<LocaleItemConfig>>(configTextAsset);
                if (listConfig != null)
                {
                    for (var i = 0; i < listConfig.Count; i++)
                    {
                        listConfig[i].Key = StringTransformer.Transform(listConfig[i].Key);
                        listConfig[i].Value = StringTransformer.Transform(listConfig[i].Value);
                    }

                    var newContent = JsonConvert.SerializeObject(listConfig);

                    //添加额外后缀
                    var savePath = Path.Combine(Application.dataPath, rootFolder,
                        $"{textFileName.Replace("locale_", "")}_ios.bytes");

                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    
                    //文件加密之后，换一文件名存储
                    File.WriteAllText(savePath, newContent);
                    //删除老的未加密的配置
                   
                    File.Delete(file);

                    if (File.Exists(file + ".meta"))
                    {
                        File.Delete(file + ".meta");
                    }
                }
            }

        }
#endif
        AssetDatabase.Refresh();
    }

    public static void WriteBundleVersionToGameModuleCode()
    {
        var globalSettingText =
            File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/WonderlandLauncher/GlobalSetting.cs"));

        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+serverVersion\\s+=\\s+\"v[\\d]*\";",
            $"protected static string serverVersion = \"{BuildVersionSetting.serverVersion}\";");
        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+resourcesVersion\\s+=\\s+\"v[\\d]*\";",
            $"protected static string resourcesVersion = \"{BuildVersionSetting.resourcesVersion}\";");
        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+resourcesRootVersion\\s+=\\s+\"rv[\\d]*\";",
            $"protected static string resourcesRootVersion = \"{BuildVersionSetting.resourcesRootVersion}\";");

        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+version\\s+=\\s+\"[\\d]*.[\\d]*.[\\d]\";",
            $"protected static string version = \"{BuildVersionSetting.version}\";");

        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+versionCode\\s+=\\s+\"[\\d]*\";",
            $"protected static string versionCode = \"{BuildVersionSetting.versionCode}\";");

        var timeStamp = System.DateTime.Now.ToString("yy-MM-dd HH:mm:ss");

        globalSettingText = Regex.Replace(globalSettingText,
            "protected\\s+static\\s+string\\s+hotfixStamp\\s+=\\s+\"[\\d]*\";",
            $"protected static string hotfixStamp = \"{timeStamp}\";");

        File.WriteAllText(Path.Combine(Application.dataPath, "Scripts/WonderlandLauncher/GlobalSetting.cs"), globalSettingText);

        AssetDatabase.SaveAssets();

        AssetDatabase.ImportAsset("Assets/Scripts/WonderlandLauncher/GlobalSetting.cs",
            ImportAssetOptions.ForceSynchronousImport);
       
        WriteTimeStamp();
    }

    public static void WriteTimeStamp()
    {
        var timeStamp = System.DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        
        var uiSettingText =
            File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/GamePlay/Setting/UISettingWidget.cs"));
        
        uiSettingText = Regex.Replace(uiSettingText,
            "public\\s+static\\s+string\\s+hotFixTimeStamp\\s+=\\s+\"[\\d]*\";",
            $"public static string hotFixTimeStamp = \"{timeStamp}\";");
        
        File.WriteAllText(Path.Combine(Application.dataPath, "Scripts/GamePlay/Setting/UISettingWidget.cs"), uiSettingText);
        
        AssetDatabase.SaveAssets();

        AssetDatabase.ImportAsset("Assets/Scripts/GamePlay/Setting/UISettingWidget.cs",
            ImportAssetOptions.ForceSynchronousImport);
    }

    // public static void CreateSoftLinkForLatestBundle(string latestBundleVersion)
    // {
    //     DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
    //     string rootPath = directoryInfo.Parent.ToString();
    //     
    //     rootPath = $"{rootPath}/ServerData/{BundleFolderSetting.BundleFolderName}/{BundleFolderSetting.BundleRootFolderName}";
    //   
    //     if (!Directory.Exists(rootPath))
    //     {
    //         Directory.CreateDirectory(rootPath);
    //     }
    //     
    //     var cmd = $"ln -s {latestBundleVersion} current_version"; 
    //     ShellHelper.ProcessCommand(cmd,rootPath);
    // }

 
    private static void CopyAssemblyToRightPath(bool onlyHotUpdate = true)
    {
        var sourcePath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);

        for (var i = 0; i < AssemblyDefine.HotAssembly.Count; i++)
        {
            var sourceFile = Path.Combine(sourcePath, AssemblyDefine.HotAssembly[i]);
            if (File.Exists(sourceFile))
            {
                var dest = $"Assets/AssetInApp/Code/{AssemblyDefine.HotAssembly[i]}.bytes";
                
                
                File.Copy(sourceFile, dest, true);
                Debug.Log("Copy:" + sourceFile + " To: " + dest);
            }
        }

        if (!onlyHotUpdate)
        {
            var aotSourcePath = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);

            for (var i = 0; i < AssemblyDefine.AOTAssembly.Count; i++)
            {
                var sourceFile = Path.Combine(aotSourcePath, AssemblyDefine.AOTAssembly[i]);
                if (File.Exists(sourceFile))
                {
                    if (!Directory.Exists("Assets/Resources/AOTMeta"))
                    {
                        Directory.CreateDirectory("Assets/Resources/AOTMeta");
                    }
                    
                    var dest = $"Assets/Resources/AOTMeta/{AssemblyDefine.AOTAssembly[i]}.bytes";
                    
                    File.Copy(sourceFile, dest,true);
                    Debug.Log("Copy:" + sourceFile + " To: " + dest);
                }
            }
        }
        
        AssetDatabase.Refresh();
    }

    private static string GetAndroidBuildOutPutPath()
    {
        string dirPath = Application.dataPath;

        DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);

        string dir = directoryInfo.Parent.ToString();

        string outputDir = Path.Combine(dir, "AndroidExport");
        return outputDir;
    }

    public static void Empty(string path)
    {
        if (Directory.Exists(path))
        {
            var directory = new DirectoryInfo(path);
            foreach (System.IO.FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                subDirectory.Delete(true);

            Directory.Delete(path);
            if (File.Exists(path + ".meta"))
            {
                File.Delete(path + ".meta");
            }
        }
    }

    private static void GenerateAll(bool isDebug = false)
    {
        SetUpSymbolDefine(isDebug);
        HybridCLR_GenerateAll(isDebug);
     
        CopyAssemblyToRightPath(false);
        BuildAssetBundle(EditorUserBuildSettings.activeBuildTarget, isDebug,true);
    }

    private static void CompileDLL(bool isDebug = false)
    {
        SetUpSymbolDefine(isDebug);
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);
        CopyAssemblyToRightPath(true);
    }
 
    private static async void BuildPlayer(BuildTarget target, bool isDebug, bool CLRBinding, bool aab)
    {
        //AssetBundle
        // BuildAssetBundle(target, isDebug);
        // if(CLRBinding)
        //     ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis();
        //  

        Debug.Log("Debug: GameModuleAssembly And Code");
        string defineSymbolString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        defineSymbolString += ";FORCE_COMP";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
            defineSymbolString);

        CopyInPackageAssetBundle(aab);
        Debug.Log("CopyInPackageAssetBundle");
        //DragonSDK
        SetupDragonSDK(isDebug);
        
        
        Debug.Log("SetupDragonSDK");
        //Mass Setting
        PlayerSettings.bundleVersion = BuildVersionSetting.version;
        PlayerSettings.Android.bundleVersionCode = int.Parse(BuildVersionSetting.versionCode);
        PlayerSettings.iOS.buildNumber = BuildVersionSetting.versionCode;


        try
        {
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
        catch (Exception e)
        {
            // ignored
        }

        if (target == BuildTarget.Android)
        {
           
            // if (!aab)
            // {
            //     ConfigurationController.Instance.Res_Server_URL_Release =
            //         ConfigurationController.Instance.Res_Server_URL_AdHoc;
            // }
        }
        else if (target == BuildTarget.iOS)
        {
           
        }

       // EditorUtility.SetDirty(AssetConfigController.Instance);
        //UnityEditor.Scripting.ScriptCompilation.D
        // Empty(Path.Combine(Application.dataPath, "Scripts/Code@GameModule~"));
        // Empty(Path.Combine(Application.dataPath, "Editor/GameModule~"));
        // System.IO.Directory.Move(Path.Combine(Application.dataPath, "Scripts/Code@GameModule") + "", Path.Combine(Application.dataPath, "Scripts/Code@GameModule~"));
        // System.IO.Directory.Move(Path.Combine(Application.dataPath, "Editor/GameModule") + "", Path.Combine(Application.dataPath, "Editor/GameModule~"));

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log("SaveAssetsDone");

        List<string> levels = new List<string>();

        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                levels.Add(EditorBuildSettings.scenes[i].path);
            }
        }
        // await AccountEditorUtility.SetupThirdPartyLibraries();
        
        
        if (target == BuildTarget.Android)
        {
            // PlayerSettings.Android.useCustomKeystore = ConfigurationController.Instance.AndroidKeyStoreUseConfiguration;
            // PlayerSettings.Android.keystoreName = ConfigurationController.Instance.AndroidKeyStorePath;
            // PlayerSettings.Android.keystorePass = ConfigurationController.Instance.AndroidKeyStorePass;
            // PlayerSettings.Android.keyaliasName = ConfigurationController.Instance.AndroidKeyStoreAlias;
            // PlayerSettings.Android.keyaliasPass = ConfigurationController.Instance.AndroidKeyStoreAliasPass;

            //Installation package
            string fileNameSuffix = aab ? ".aab" : ".apk";
            EditorUserBuildSettings.buildAppBundle = aab;

            string fileName = "MergeMatch1" + fileNameSuffix;

            string outputDir = GetAndroidBuildOutPutPath();
            string outputPath = outputDir + $"/{fileName}";

            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            Debug.Log($"==========outputPath:{outputPath}");

            if (aab)
            {
                Debug.Log("Start:BuildPlayerAAB");
                var result = BuildPlayerWithAssetPack(outputPath, isDebug);
                Debug.LogError(result.ToString());
            }
            else
            {
                Debug.Log("Start:BuildPlayer");
                BuildReport report = BuildPipeline.BuildPlayer(levels.ToArray(), outputPath, target,
                    isDebug ? BuildOptions.Development : BuildOptions.None);
                BuildSummary summary = report.summary;
                Debug.LogError(summary.result.ToString());

                Debug.Log("Finish:BuildPlayer");
            }
        }
        else if (target == BuildTarget.iOS)
        {
            
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            string outputPath = Path.GetFullPath(Application.dataPath + "/../iOS/build/");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Debug.Log($"==========outputPath:{outputPath}");

            BuildReport report = BuildPipeline.BuildPlayer(levels.ToArray(), outputPath, target,
                isDebug ? BuildOptions.Development : BuildOptions.None);
            BuildSummary summary = report.summary;

            Debug.LogError(summary.result.ToString());
        }
        else
        {
            Debug.LogError($"暂时不支持平台：{target}");
        }
        // Empty(Path.Combine(Application.dataPath, "Scripts/Code@GameModule"));
        // Empty(Path.Combine(Application.dataPath, "Editor/GameModule"));
        // System.IO.Directory.Move(Path.Combine(Application.dataPath, "Scripts/Code@GameModule~") + "", Path.Combine(Application.dataPath, "Scripts/Code@GameModule"));
        // System.IO.Directory.Move(Path.Combine(Application.dataPath, "Editor/GameModule~") + "", Path.Combine(Application.dataPath, "Editor/GameModule"));

        Debug.Log("BuildPlayer......Done");
        EditorApplication.Exit(0);
    }

    private static bool BuildPlayerWithAssetPack(string path, bool isDebug)
    {
        var buildPlayerOptions = AndroidBuildHelper.CreateBuildPlayerOptions(path.Replace(".apk", ".aab"));

        // BuildReport report = BuildPipeline.BuildPlayer(levels.ToArray(), outputPath, target, isDebug ? BuildOptions.Development : BuildOptions.None);

        buildPlayerOptions.options = isDebug ? BuildOptions.Development : BuildOptions.None;
        AssetPackConfig assetPackConfig = new AssetPackConfig();

        //  AddInstallTimeAssetsFolders(assetPackConfig);
        string androidAssetPackOutPutPath = GetAndroidBuildOutPutPath() + "/AssetPack";

        // 如果签名文件不在项目中，则PlayerSettings.Android.keystoreName只保留了文件名
        // 导致打包找不到签名文件，所以这里复制一份来使用

        assetPackConfig.AddAssetsFolder("InstallTimeAssetPack", androidAssetPackOutPutPath,
            AssetPackDeliveryMode.InstallTime);
        System.Threading.Thread.Sleep(1000);

        // if (!string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) &&
        //     ConfigurationController.Instance.AndroidKeyStorePath != PlayerSettings.Android.keystoreName)
        // {
        //     Debug.Log("BuildPlayerWithAssetPack Copy Keystore Src:" +
        //               ConfigurationController.Instance.AndroidKeyStorePath + " Dst:" +
        //               PlayerSettings.Android.keystoreName);
        //     SDKUtil.FilePath.CopyFile(ConfigurationController.Instance.AndroidKeyStorePath,
        //         PlayerSettings.Android.keystoreName);
        // }

        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;

        var result = Bundletool.BuildBundle(buildPlayerOptions, assetPackConfig, true);

        // if (!string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) &&
        //     ConfigurationController.Instance.AndroidKeyStorePath != PlayerSettings.Android.keystoreName)
        // {
        //     SDKUtil.FilePath.DeleteFile(PlayerSettings.Android.keystoreName);
        // }

        return result;
    }

    public static void SetupDragonSDK(bool isDebug)
    {
        // ThirdPartySetUpTools.SetUpThirdPartySetting();
        //
        // if (!isDebug)
        // {
        //     AppLovinSettings.Instance.QualityServiceEnabled = true;
        //
        //     var maxConfig = SDK<ThirdPartyConfigProvider>.Instance.GetConfigInfo<MaxConfigInfo>(ThirdParty.MAX);
        //     AppLovinSettings.Instance.SdkKey = maxConfig.sdkKey;
        //
        //     var admobConfig = SDK<ThirdPartyConfigProvider>.Instance.GetConfigInfo<AdmobConfigInfo>(ThirdParty.Admob);
        //     AppLovinSettings.Instance.AdMobAndroidAppId = admobConfig.AndroidAppID;
        //     AppLovinSettings.Instance.AdMobIosAppId = admobConfig.iOSAppID;
        //
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        // }
    }

    private static void CopyInPackageAssetBundle(bool aab)
    {
        string packageBuildInAssetBundle = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        
        string androidAssetPackOutPutPath = GetAndroidBuildOutPutPath() + "/AssetPack";

        if (aab)
        {
            var outPutPath = GetAndroidBuildOutPutPath();
            if (!Directory.Exists(outPutPath))
                Directory.CreateDirectory(outPutPath);

            Directory.Move(packageBuildInAssetBundle, androidAssetPackOutPutPath);
        }
    }
    
    private class VersionInfo
    {
        public string Version;
    }

    // [MenuItem("NewBuild/Config/CreateResVersionTxt")]
    private static void CreateResVersionTxt()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
        string rootPath = directoryInfo.Parent.ToString();
        rootPath = $"{rootPath}/ServerData/{BuildVersionSetting.bundleFolderName}";
       
        if (!Directory.Exists(rootPath)) 
            Directory.CreateDirectory(rootPath);

        var versionInfo = new VersionInfo();
        versionInfo.Version = $"{BuildVersionSetting.resourcesRootVersion}/{BuildVersionSetting.resourcesVersion}";

        // var json = JsonConvert.SerializeObject(versionInfo);

        string versionFile = $"{rootPath}/Version.{BuildVersionSetting.resourcesVersion.Replace("v","")}.txt";
        if (!File.Exists(versionFile))
        {
            var fileStream = File.Create(versionFile);
            fileStream.Close();
        }

        // File.WriteAllText(versionFile, json);
    }
}
