using System;
using System.IO;
using System.Text;
using Storage.StorageGenerateClass;
using UnityEngine;

public class StorageManager
{
   private static StorageManager instance;
   public static StorageManager Instance
   {
      get
      {
         if (instance == null)
            instance = new StorageManager();
         return instance;
      }
   }
   public static StorageBase FromJson(string jsonStr,int index,out int endIndex)
   {
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

   public StorageRoot Root;
   private string StorageSavePath => Path.Combine(Application.persistentDataPath,"storage.json");
   public void SaveToFile()
   {
      var sb = new StringBuilder();
      Root.GetInnerStorage().Json(sb);
      File.WriteAllText(StorageSavePath, sb.ToString());
   }
    
   public void LoadFromFile()
   {
      if (File.Exists(StorageSavePath))
      {
         string jsonData = File.ReadAllText(StorageSavePath);
         Root.GetInnerStorage().FromJson(jsonData,0,out var endIndex);
      }
      else
      {
         Debug.Log("存档文件不存在,创建新存档");
         Root = new StorageRoot();
         SaveToFile();
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