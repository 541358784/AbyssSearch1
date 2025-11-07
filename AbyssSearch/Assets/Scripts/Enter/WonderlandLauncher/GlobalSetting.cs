// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2023/06/25/11:12
// Ver : 1.0.0
// Description : GloabalSetting.cs
// ChangeLog :
// **********************************************

using YooAsset;

namespace Wonderland
{
    public class GlobalSetting
    {
        //don't made any change to the follow config
        //config updated by app builder

        /// <summary>
        /// app 底包的版本号：例如 1.0.1
        /// </summary>
        protected static string version = "1.0.15";
        
        /// <summary>
        /// app 底包的版本代码：例如 3
        /// </summary>
        protected static string versionCode = "16";
        
        protected static string serverVersion = "v1";
        
        protected static string resourcesVersion = "v34";
        
        protected static string resourcesRootVersion = "rv9";
        
#if UNITY_IOS
        public const string BundleFolderName = "ios";
#elif UNITY_ANDROID
        public const string BundleFolderName = "android";
#endif        
        public static string DefaultPackageName => "DefaultPackage";
        
        public static string FetchedRootVersion = "";
        
        public static string ServerVersion
        {
            get { return serverVersion; }
            set { serverVersion = value; }
        }

        public static string ResourceVersion
        {
            get { return resourcesVersion; }
            set { resourcesVersion = value; }
        }

        public static string RootVersion => resourcesRootVersion;

        public static string VersionCode => versionCode;
        public static string Version => version;

        public static string GetHostResUrl()
        {
            return "127.0.0.1" + $"{BundleFolderName}/{resourcesRootVersion}/{resourcesVersion}";
        }

        public static EPlayMode PlayMode
        {
            get
            {
#if UNITY_EDITOR
                return EPlayMode.EditorSimulateMode;
#else
                return EPlayMode.HostPlayMode;
#endif
            } 
        }
    }
}