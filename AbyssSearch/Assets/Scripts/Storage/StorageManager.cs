using System;
using System.IO;
using System.Text;
using Storage.StorageGenerateClass;
using UnityEngine;

public class StorageManager
{
   private static StorageManager instance;
   private static readonly object lockObject = new object();
   
   public static StorageManager Instance
   {
      get
      {
         if (instance == null)
         {
            lock (lockObject)
            {
               if (instance == null)
                  instance = new StorageManager();
            }
         }
         return instance;
      }
   }
   
   public static StorageBase FromJson(string jsonStr,int index,out int endIndex)
   {
      if (string.IsNullOrEmpty(jsonStr))
      {
         endIndex = index;
         return null;
      }
      
      if (index < 0 || index >= jsonStr.Length)
      {
         endIndex = index;
         return null;
      }
      
      // 跳过空白字符
      while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
         index++;
      
      if (index >= jsonStr.Length)
      {
         endIndex = index;
         return null;
      }
      
      switch (jsonStr[index])
      {
         case '\"':
         {
            var storage = new StorageBaseString();
            storage.FromJson(jsonStr,index,out endIndex);
            return storage;
         }
         case '{':
         {
            var storage = new StorageBaseDictionary();
            storage.FromJson(jsonStr,index,out endIndex);
            return storage;
         }
         case '[':
         {
            var storage = new StorageBaseList();
            storage.FromJson(jsonStr,index,out endIndex);
            return storage;
         }
         default:
         {
            endIndex = index;
            return null;  
         }
      }
   }

   public StorageRoot Root = new StorageRoot();
   private string StorageSavePath => Path.Combine(Application.persistentDataPath,"storage.json");
   private string StorageBackupPath => Path.Combine(Application.persistentDataPath,"storage.json.backup");
   private string StorageTempPath => Path.Combine(Application.persistentDataPath,"storage.json.tmp");
   
   public void SaveToFile()
   {
      try
      {
         lock (lockObject)
         {
            // 先保存到临时文件
            var sb = new StringBuilder();
            Root.GetInnerStorage().Json(sb);
            string jsonContent = sb.ToString();
            
            // 写入临时文件
            File.WriteAllText(StorageTempPath, jsonContent);
            
            // 如果原文件存在，先备份
            if (File.Exists(StorageSavePath))
            {
               File.Copy(StorageSavePath, StorageBackupPath, true);
            }
            
            // 将临时文件移动到正式文件
            if (File.Exists(StorageTempPath))
            {
               File.Move(StorageTempPath, StorageSavePath);
            }
            
            Debug.Log($"存档保存成功: {StorageSavePath}");
         }
      }
      catch (Exception ex)
      {
         Debug.LogError($"存档保存失败: {ex.Message}\n{ex.StackTrace}");
         throw;
      }
   }
    
   public void LoadFromFile()
   {
      try
      {
         if (File.Exists(StorageSavePath))
         {
            string jsonData = File.ReadAllText(StorageSavePath);
            if (string.IsNullOrEmpty(jsonData))
            {
               Debug.LogWarning("存档文件为空，创建新存档");
               SaveToFile();
               return;
            }
            
            Root.GetInnerStorage().FromJson(jsonData,0,out var endIndex);
            Debug.Log($"存档加载成功: {StorageSavePath}");
         }
         else
         {
            Debug.Log("存档文件不存在,创建新存档");
            SaveToFile();
         }
      }
      catch (Exception ex)
      {
         Debug.LogError($"存档加载失败: {ex.Message}\n{ex.StackTrace}");
         
         // 尝试从备份恢复
         if (File.Exists(StorageBackupPath))
         {
            try
            {
               Debug.LogWarning("尝试从备份文件恢复...");
               string backupData = File.ReadAllText(StorageBackupPath);
               Root.GetInnerStorage().FromJson(backupData,0,out var endIndex);
               Debug.Log("从备份文件恢复成功");
               SaveToFile(); // 重新保存
            }
            catch (Exception backupEx)
            {
               Debug.LogError($"从备份恢复也失败: {backupEx.Message}");
               Debug.Log("创建新存档");
               SaveToFile();
            }
         }
         else
         {
            Debug.Log("创建新存档");
            SaveToFile();
         }
      }
   }

   [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
   public static void OnGameStart()
   {
      Instance.CreateStorage();
   }
   
   public void CreateStorage()
   {
      LoadFromFile();
   }
}