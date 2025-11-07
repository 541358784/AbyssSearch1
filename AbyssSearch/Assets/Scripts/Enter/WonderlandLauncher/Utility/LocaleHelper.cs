// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2023/08/17/10:17
// Ver : 1.0.0
// Description : LocaleHelper.cs
// ChangeLog :
// **********************************************

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wonderland.Utility
{
    public class LocaleHelper
    {
        private static string current_locale = Locale.ENGLISH;

        private static bool localeMatched = false;
        public static class Locale
        {
            public const string NONE = "none";
            public const string ENGLISH = "en";
            public const string FRENCH = "fr";
            public const string GERMAN = "de";
            public const string PORTUGUESE = "pt";
            public const string SPANISH = "es";
            public const string ITALIAN = "it";
            public const string INDONESIAN = "id";
            public const string RUSSIAN = "ru";
            public const string VIETNAMESE = "vi";
            public const string TURKISH = "tr";
            public const string THAI = "th";
            public const string JAPANESE = "jp";
            public const string KOREA = "kr";
            public const string CHINESE_SIMPLIFIED = "zh";
            public const string CHINESE_TRADITION = "zht";
            public const string HINDI = "hi";
            public const string DUTCH = "nl"; // 荷兰
            public const string MALAYSIA = "ms"; // 马来西亚
            public const string ARABIC = "ar"; // 阿拉伯
            public const string Dutch = "nl";// 荷兰
        }
        private static readonly StringBuilder m_builder = new StringBuilder();
        
        private static string Format(string s, params object[] values)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (values.Length < 1) return s; // params is empty

            int vi = 0,
                start = 0,
                now = 0,
                len = s.Length; // value index, current start position, current search position

           // lock (m_lock)
            {
                m_builder.Clear();
                while (now < len)
                {
                    if (s[now++] != '%') continue;
                    var c = s[now++];
                    if (c == 's' || c == 'S')
                    {
                        var v = values[vi++];
                        m_builder.Append(s.Substring(start, now - start - 2))
                            .Append(v == null ? string.Empty : v.ToString());
                        start = now;

                        if (vi >= values.Length) break;
                    }
                }

                if (start < len) m_builder.Append(s.Substring(start));

                return m_builder.ToString();
            }
        }


        public static Dictionary<string, string> initializedTexts = new Dictionary<string, string>()
        {
            {"en", "Initializing…%s%"},
            {"de", "Initialisieren…%s%"},
            {"fr", "Initialisation…%s%"},
            {"kr", "초기화 중...%s%"},
            {"jp", "初期化中...%s%"},
            {"es", "Inicializando…%s%"},
            {"pt", "Inicializando…%s%"},
            {"zh", "初始化中…%s%"},
            {"zht", "初始化中…%s%"},
            {"ru", "Инициализация...%s%"},
            // {"it", "Inizializzazione...%s%"},
            // {"id", "Menginisialisasi...%s%"},
            //
            // {"vi", "Đang khởi tạo…%s%"},
            // {"tr", "Başlatılıyor…%s%"},
            // {"th", "กำลังเริ่มต้น...%s%"},
        };
        
        public static readonly List<string> supportedLocale = new List<string>
        {
            Locale.ENGLISH,
            Locale.GERMAN,
            Locale.FRENCH,
            Locale.KOREA,
            Locale.JAPANESE,
            Locale.SPANISH,
            Locale.PORTUGUESE,
            Locale.CHINESE_SIMPLIFIED,
            Locale.CHINESE_TRADITION,
            Locale.RUSSIAN,
            
            //Locale.CHINESE_TRADITION,
            //Locale.ITALIAN,
            // Locale.INDONESIAN,
            // Locale.VIETNAMESE,
            // Locale.THAI,
            // Locale.HINDI,
            // Locale.DUTCH,
            // Locale.MALAYSIA,
            // Locale.ARABIC,
        };
        
        private static string GetSystemLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Afrikaans: return "af";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Basque: return "eu";
                case SystemLanguage.Belarusian: return "be";
                case SystemLanguage.Bulgarian: return "bg";
                case SystemLanguage.Catalan: return "ca";
                case SystemLanguage.Chinese: return "zh";
                case SystemLanguage.ChineseSimplified: return "zh";
                case SystemLanguage.ChineseTraditional: return "zht";
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Estonian: return "et";
                case SystemLanguage.Faroese: return "fo";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Icelandic: return "is";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Japanese: return "jp";
                case SystemLanguage.Korean: return "kr";
                case SystemLanguage.Latvian: return "lv";
                case SystemLanguage.Lithuanian: return "lt";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.SerboCroatian: return "hr";
                case SystemLanguage.Slovak: return "sk";
                case SystemLanguage.Slovenian: return "sl";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Vietnamese: return "vi";
                case SystemLanguage.Hungarian: return "hu";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Unknown: return "en";
            }

            return "en";
        }
         
        private static string GetSystemLanguage()
        {
            string osLanguage = GetSystemLanguage(Application.systemLanguage);
            return GetLocal(osLanguage);
        }
        
        private static string GetLocal(string language)
        {
            if (supportedLocale.Contains(language))
                return language;

            return Locale.ENGLISH;
        }

        public static void MatchLocale()
        {
            if (!localeMatched)
            {
                // var storage = SDK<IStorage>.Instance.Get<StorageCommon>();
                //
                // string userLanguage = GetSystemLanguage();
                // if (string.IsNullOrEmpty(storage.Locale))
                // {
                //     storage.Locale = userLanguage;
                //     storage.OrigLocale = userLanguage;
                //     SetCurrentLocale(userLanguage);
                // }
                // else
                // {
                //     if (supportedLocale.Contains(storage.Locale))
                //     {
                //         SetCurrentLocale(storage.Locale);
                //     }
                //     else
                //     {
                //         storage.Locale = userLanguage;
                //         storage.OrigLocale = userLanguage;
                //         SetCurrentLocale(userLanguage);
                //     }
                // }

                localeMatched = true;
            }
        }
        
        public static bool SetCurrentLocale(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return false;
            }

            if (current_locale == locale)
            {
                return false;
            }

            if (!supportedLocale.Contains(locale))
            {
                return false;
            }

            current_locale = locale;
            // SDK<IStorage>.Instance.Get<StorageCommon>().Locale = locale;
            return true;
        }

        // public static Material GetLocaleMaterial(string suffix)
        // {
        //     string locale = Utility.Text.FirstCharToUpper(current_locale);
        //     string key = string.Format("LocaleFont_{0} SDF {1}", locale, suffix);
        //     Material mat = null;
        //
        //     string path = string.Format("Export/Fonts/{0}/{1}", locale, key);
        //     //mat = ResourcesManager.Instance.LoadResource<Material>(path);
        //     mat = Resources.Load<Material>(path);
        //     return mat;
        // }

        // public static TMP_FontAsset GetLocaleFont()
        // {
        //     string locale = Utility.Text.FirstCharToUpper(current_locale);
        //
        //     string key = string.Format("LocaleFont_{0} SDF", locale);
        //
        //     string path = string.Format("Export/Fonts/{0}/{1}", locale, key);
        //     //fontAsset = ResourcesManager.Instance.LoadResource<TMP_FontAsset>(path);
        //     var fontAsset = Resources.Load<TMP_FontAsset>(path);
        //     return fontAsset;
        // }
 
        // public static void LocaleTextMeshProUGUI(TextMeshProUGUI textMeshProUGUI, string materialName = "Outline Green")
        // {
        //     MatchLocale();
        //     
        //     var localMaterial = GetLocaleMaterial(materialName);
        //     if (localMaterial != null)
        //     {
        //         textMeshProUGUI.fontMaterial = localMaterial;
        //     }
        //
        //     textMeshProUGUI.font = GetLocaleFont();
        // }

        public static string GetInitializeText(float progress)
        {
            //var initText = initializedTexts[current_locale];

            return Format("%s%", progress.ToString("f2"));
        }
    }
}