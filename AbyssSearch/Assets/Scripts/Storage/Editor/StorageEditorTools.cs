using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Storage.Editor
{
    public static class StorageEditorTools
    {
        private const string srcNamespaceName = "Storage.SrcClass";
        private const string outputPath = "Assets/Scripts/Storage/StorageGenerateClass/";
        private const string outPutNamespaceName = "Storage.StorageGenerateClass";
        
        [MenuItem("Tools/Storage代码生成")]
        private static void GenerateCodeFiles()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                // 获取所有程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var types = new List<Type>();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // 获取指定命名空间下的所有类型
                        var assemblyTypes = assembly.GetTypes()
                            .Where(t => t.Namespace != null && t.Namespace.Contains(srcNamespaceName));

                        types.AddRange(assemblyTypes);
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // 忽略无法加载类型的程序集
                        continue;
                    }
                }

                // 根据筛选条件过滤类型
                var filteredTypes = types.Where(t =>t.IsClass);
                // 为每个类型生成文件
                int generatedCount = 0;
                foreach (var type in filteredTypes)
                {
                    if (GenerateCodeFile(type))
                    {
                        generatedCount++;
                    }
                }

                // 刷新资源数据库以显示新文件
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Code Generation Complete",
                    $"Successfully generated {generatedCount} files in {outputPath}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Failed to generate code: {ex.Message}", "OK");
            }
        }

        private static bool GenerateCodeFile(Type type)
        {
            try
            {
                string fileName = $"Storage{type.Name}.cs";
                string filePath = Path.Combine(outputPath, fileName);

                // 生成代码内容
                string codeContent = GenerateCodeForType(type);

                // 写入文件
                File.WriteAllText(filePath, codeContent);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate code for {type.Name}: {ex.Message}");
                return false;
            }
        }

        private static string GenerateCodeForType(Type type)
        {
            // 这里是代码生成的核心逻辑
            // 您可以根据需要修改这部分来生成符合您需求的代码

            string className = $"Storage{type.Name}";

            // 生成属性
            var fields = type.GetFields(BindingFlags.Public|BindingFlags.Instance);
            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                Debug.LogError("field="+field);
                sb.Append(GetTypeCodeBlock(field));
            }
            var propertiesCode = sb.ToString();
            // 构建完整的代码文件
            return $@"
namespace {outPutNamespaceName}
{{
    /// <summary>
    /// 自动生成的类，基于 {type.Name} 类型
    /// </summary>
    public class {className} : StorageClass
    {{
        // 属性
{propertiesCode}
    }}
}}";
        }

        private static string GetTypeName(Type type)
        {
            var className = "Storage";
            if (type == typeof(string))
            {
                className = "StorageString";
            }
            else if (type == typeof(int))
            {
                className = "StorageInt";
            }
            else if (type == typeof(float))
            {
                className = "StorageFloat";
            }
            else if (type == typeof(long))
            {
                className = "StorageLong";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] customArgs = type.GetGenericArguments();
                className = $"StorageList<{GetTypeName(customArgs[0])}>";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] customArgs = type.GetGenericArguments();
                className = $"StorageDictionary<{GetTypeName(customArgs[0])},{GetTypeName(customArgs[1])}>";
            }
            else if (type.IsClass)
            {
                className = "Storage" + type.Name;
            }
            else
            {
                throw new Exception("类型错误,无法解析类型" + type);
            }
            return className;
        }
        private static string GetTypeCodeBlock(FieldInfo field)
        {
            var type = field.FieldType;
            var className = GetTypeName(type);
            var codeStr = "";
            if (!type.IsClass || type == typeof(string))
            {
                codeStr= $@"
        public {GetValueTypeKeyName(type)} {field.Name}
        {{
            get => GetValue<{className}>(""{field.Name}"").Value;
            set => GetValue<{className}>(""{field.Name}"").Value = value;
        }}
";   
            }
            else
            {
                codeStr= $@"
        public {className} {field.Name}
        {{
            get => GetValue<{className}>(""{field.Name}"");
            set => SetValue(""{field.Name}"", value);
        }}
";
            }
            return codeStr;
        }

        private static string GetValueTypeKeyName(Type valueType)
        {
            if (valueType == typeof(int))
            {
                return "int";
            }
            else if (valueType == typeof(float))
            {
                return "float";
            }
            else if (valueType == typeof(long))
            {
                return "long";
            }
            else if (valueType == typeof(string))
            {
                return "string";
            }
            throw new Exception("没有对应值类型关键字" + valueType);
        }
    }
}