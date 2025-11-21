using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class StorageBase
{
   public StorageBase Parent;
   private bool isDirty = true;
   public bool IsDirty
   {
      set
      {
         if (!isDirty)
         {
            isDirty = true;
            if (Parent != null)
               Parent.IsDirty = true;
         }
      }
   }
   public void Json(StringBuilder sb)
   {
      if (isDirty)
      {
         isDirty = false;
         BuildJson();
      }
      sb.Append(json);
   }

   protected string json;

   protected abstract void BuildJson();
   public abstract void FromJson(string jsonStr, int index, out int endIndex);
   
   // 辅助方法：检查边界
   protected static void CheckBounds(string jsonStr, int index, string context)
   {
      if (index < 0 || index >= jsonStr.Length)
         throw new Exception($"{context}: 索引越界 index={index}, length={jsonStr.Length}");
   }
   
   // 辅助方法：转义JSON字符串
   protected static void EscapeJsonString(StringBuilder sb, string str)
   {
      if (str == null) return;
      foreach (char c in str)
      {
         switch (c)
         {
            case '"': sb.Append("\\\""); break;
            case '\\': sb.Append("\\\\"); break;
            case '\n': sb.Append("\\n"); break;
            case '\r': sb.Append("\\r"); break;
            case '\t': sb.Append("\\t"); break;
            default: sb.Append(c); break;
         }
      }
   }
   
   // 辅助方法：解析转义字符串
   protected static string UnescapeJsonString(string jsonStr, ref int index)
   {
      StringBuilder sb = new StringBuilder();
      while (index < jsonStr.Length)
      {
         if (jsonStr[index] == '\\' && index + 1 < jsonStr.Length)
         {
            index++; // 跳过 '\'
            switch (jsonStr[index])
            {
               case '"': sb.Append('"'); break;
               case '\\': sb.Append('\\'); break;
               case 'n': sb.Append('\n'); break;
               case 'r': sb.Append('\r'); break;
               case 't': sb.Append('\t'); break;
               default: sb.Append('\\').Append(jsonStr[index]); break;
            }
            index++;
         }
         else if (jsonStr[index] == '"')
         {
            break;
         }
         else
         {
            sb.Append(jsonStr[index]);
            index++;
         }
      }
      return sb.ToString();
   }
}
public class StorageBaseDictionary:StorageBase,IEnumerable<KeyValuePair<string,StorageBase>>
{
   private Dictionary<string, StorageBase> InnerDictionary = new Dictionary<string, StorageBase>();
   protected override void BuildJson()
   {
      StringBuilder sb = new StringBuilder();
      sb.Append('{');
      bool first = true;
      foreach (var pair in InnerDictionary)
      {
         if (!first) sb.Append(',');
         first = false;
         
         // 标准JSON格式：键需要引号，使用冒号
         sb.Append('"');
         EscapeJsonString(sb, pair.Key);
         sb.Append("\":");
         pair.Value?.Json(sb);
      }
      sb.Append('}');
      json = sb.ToString();
   }
   public int Count => InnerDictionary.Count;
   public void Clear()
   {
      foreach (var pair in InnerDictionary)
      {
         pair.Value.Parent = null;
      }
      InnerDictionary.Clear();
      IsDirty = true;
   }

   public List<string> GetKeys()
   {
      return InnerDictionary.Keys.ToList();
   }

   public IEnumerator<KeyValuePair<string,StorageBase>> GetEnumerator()
   {
      foreach (var pair in InnerDictionary)
      {
         yield return pair;
      }
   }
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
   public bool ContainsKey(string key)
   {
      return InnerDictionary.ContainsKey(key);
   }
   public void SetValue(string key,StorageBase value)
   {
      if (value == null)
         throw new ArgumentNullException(nameof(value));
         
      if (InnerDictionary.ContainsKey(key))
      {
         InnerDictionary[key].Parent = null;
         InnerDictionary[key] = value;
         InnerDictionary[key].Parent = this;
         IsDirty = true;
      }
      else
      {
         Add(key, value);
      }
   }

   public void Remove(string key)
   {
      if (InnerDictionary.ContainsKey(key))
      {
         InnerDictionary[key].Parent = null;
         InnerDictionary.Remove(key);
         IsDirty = true;
      }
   }

   public void Add(string key,StorageBase value)
   {
      if (value == null)
         throw new ArgumentNullException(nameof(value));
         
      InnerDictionary.Add(key,value);
      InnerDictionary[key].Parent = this;
      IsDirty = true;
   }
   
   public StorageBase GetValue(string key)
   {
      if (InnerDictionary.ContainsKey(key))
      {
         return InnerDictionary[key];
      }
      return null;
   }
   
   public override void FromJson(string jsonStr,int index,out int endIndex)
   {
      CheckBounds(jsonStr, index, "字典解析");
      
      if (jsonStr[index] != '{')
         throw new Exception($"字典解析错误: 首位不为{{, index={index}, char='{jsonStr[index]}'");
      
      index++; // 跳过 '{'
      InnerDictionary.Clear();
      
      // 跳过空白字符
      while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
         index++;
      
      // 空字典
      if (index < jsonStr.Length && jsonStr[index] == '}')
      {
         endIndex = index + 1;
         IsDirty = true;
         return;
      }
      
      while (index < jsonStr.Length)
      {
         CheckBounds(jsonStr, index, "字典解析-键");
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         if (index >= jsonStr.Length || jsonStr[index] == '}')
            break;
            
         // 解析键（应该是引号字符串）
         if (jsonStr[index] != '"')
            throw new Exception($"字典解析错误: 键应该以\"开始, index={index}, char='{jsonStr[index]}'");
         
         index++; // 跳过开始的 '"'
         string key = UnescapeJsonString(jsonStr, ref index);
         
         if (index >= jsonStr.Length || jsonStr[index] != '"')
            throw new Exception($"字典解析错误: 键未正确结束, index={index}");
         
         index++; // 跳过结束的 '"'
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         CheckBounds(jsonStr, index, "字典解析-冒号");
         
         if (jsonStr[index] != ':')
            throw new Exception($"字典解析错误: 键值对之间应该用:分隔, index={index}, char='{jsonStr[index]}'");
         
         index++; // 跳过 ':'
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         // 解析值
         var value = StorageManager.FromJson(jsonStr, index, out index);
         if (value == null)
            throw new Exception($"字典解析错误: 无法解析值, index={index}");
            
         value.Parent = this;
         InnerDictionary.Add(key, value);
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         // 检查是否有逗号或结束
         if (index >= jsonStr.Length)
            throw new Exception($"字典解析错误: 未找到结束符}}, index={index}");
            
         if (jsonStr[index] == '}')
            break;
            
         if (jsonStr[index] != ',')
            throw new Exception($"字典解析错误: 元素之间应该用,分隔, index={index}, char='{jsonStr[index]}'");
         
         index++; // 跳过 ','
      }
      
      CheckBounds(jsonStr, index, "字典解析-结束");
      
      if (jsonStr[index] != '}')
         throw new Exception($"字典解析错误: 未找到结束符}}, index={index}, char='{jsonStr[index]}'");
      
      index++; // 跳过 '}'
      endIndex = index;
      IsDirty = true;
   }
}
public class StorageBaseList : StorageBase ,IEnumerable<StorageBase>
{
   private List<StorageBase> InnerList = new List<StorageBase>();
   protected override void BuildJson()
   {
      StringBuilder sb = new StringBuilder();
      sb.Append('[');
      bool first = true;
      foreach (var item in InnerList)
      {
         if (!first) sb.Append(',');
         first = false;
         item?.Json(sb);
      }
      sb.Append(']');
      json = sb.ToString();
   }
   public int Count => InnerList.Count;
   public IEnumerator<StorageBase> GetEnumerator()
   {
      foreach (var item in InnerList)
      {
         yield return item;
      }
   }
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
   public void Add(StorageBase value)
   {
      if (value == null)
         throw new ArgumentNullException(nameof(value));
         
      InnerList.Add(value);
      value.Parent = this;
      IsDirty = true;
   }

   public void Clear()
   {
      foreach (var child in InnerList)
      {
         child.Parent = null;
      }
      InnerList.Clear();
      IsDirty = true;
   }
   public void RemoveAt(int index)
   {
      if (index >= 0 && index < InnerList.Count)
      {
         InnerList[index].Parent = null;
         InnerList.RemoveAt(index);
         IsDirty = true;
      }
   }

   public void Insert(int index,StorageBase value)
   {
      if (value == null)
         throw new ArgumentNullException(nameof(value));
         
      if (index >= 0 && index <= InnerList.Count)
      {
         InnerList.Insert(index,value);
         value.Parent = this;
         IsDirty = true;
      }
   }
   public void SetValue(int index,StorageBase value)
   {
      if (value == null)
         throw new ArgumentNullException(nameof(value));
         
      if (index >= 0 && index < InnerList.Count)
      {
         InnerList[index].Parent = null;
         InnerList[index] = value;
         InnerList[index].Parent = this;
         IsDirty = true;
      }
      else if (index == InnerList.Count)
         Add(value);
   }
   public StorageBase GetValue(int index)
   {
      if (index >= 0 && index < InnerList.Count)
      {
         return InnerList[index];
      }
      return null;
   }
   
   public override void FromJson(string jsonStr,int index,out int endIndex)
   {
      CheckBounds(jsonStr, index, "列表解析");
      
      if (jsonStr[index] != '[')
         throw new Exception($"列表解析错误: 首位不为[, index={index}, char='{jsonStr[index]}'");
      
      index++; // 跳过 '['
      InnerList.Clear();
      
      // 跳过空白字符
      while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
         index++;
      
      // 空列表
      if (index < jsonStr.Length && jsonStr[index] == ']')
      {
         endIndex = index + 1;
         IsDirty = true;
         return;
      }
      
      while (index < jsonStr.Length)
      {
         CheckBounds(jsonStr, index, "列表解析-元素");
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         if (index >= jsonStr.Length || jsonStr[index] == ']')
            break;
         
         var value = StorageManager.FromJson(jsonStr, index, out index);
         if (value == null)
            throw new Exception($"列表解析错误: 无法解析元素, index={index}");
            
         value.Parent = this;
         InnerList.Add(value);
         
         // 跳过空白字符
         while (index < jsonStr.Length && char.IsWhiteSpace(jsonStr[index]))
            index++;
         
         if (index >= jsonStr.Length)
            throw new Exception($"列表解析错误: 未找到结束符], index={index}");
            
         if (jsonStr[index] == ']')
            break;
            
         if (jsonStr[index] != ',')
            throw new Exception($"列表解析错误: 元素之间应该用,分隔, index={index}, char='{jsonStr[index]}'");
         
         index++; // 跳过 ','
      }
      
      CheckBounds(jsonStr, index, "列表解析-结束");
      
      if (jsonStr[index] != ']')
         throw new Exception($"列表解析错误: 未找到结束符], index={index}, char='{jsonStr[index]}'");
      
      index++; // 跳过 ']'
      endIndex = index;
      IsDirty = true;
   }
}
public class StorageBaseString : StorageBase
{
   public StorageBaseString(string str="")
   {
      SetValue(str);
   }
   private string InnerString=null;
   protected override void BuildJson()
   {
      StringBuilder sb = new StringBuilder();
      sb.Append('"');
      if (InnerString != null)
         EscapeJsonString(sb, InnerString);
      sb.Append('"');
      json = sb.ToString();
   }

   public void SetValue(string value)
   {
      if (InnerString == value)
         return;
      InnerString = value ?? string.Empty;
      IsDirty = true;
   }

   public string GetValue()
   {
      return InnerString ?? string.Empty;
   }

   public override void FromJson(string jsonStr,int index,out int endIndex)
   {
      CheckBounds(jsonStr, index, "字符串解析");
      
      if (jsonStr[index] != '"')
         throw new Exception($"字符串解析错误: 首位不为\", index={index}, char='{jsonStr[index]}'");
      
      index++; // 跳过开始的 '"'
      InnerString = UnescapeJsonString(jsonStr, ref index);
      
      if (index >= jsonStr.Length || jsonStr[index] != '"')
         throw new Exception($"字符串解析错误: 未找到结束符\", index={index}");
      
      index++; // 跳过结束的 '"'
      endIndex = index;
      IsDirty = true;
   }
}
public interface IStorageContainer
{
   public void Init(StorageBase storage = null);
   public StorageBase GetInnerStorage();
}
public class StorageBaseStringContainer:IStorageContainer
{
   protected StorageBaseString InnerStorageBaseString;

   public StorageBase GetInnerStorage()
   {
      return InnerStorageBaseString;
   }
   public StorageBaseStringContainer(StorageBaseString StorageBaseString= null)
   {
      Init(StorageBaseString);
   }
   public StorageBaseStringContainer()
   {
      Init();
   }
   public void Init(StorageBase StorageBaseDictionary = null)
   {
      if (StorageBaseDictionary == null)
      {
         InnerStorageBaseString = new StorageBaseString();
      }
      else
      {
         InnerStorageBaseString = (StorageBaseString)StorageBaseDictionary;  
      }
   }
}
public class StorageBaseListContainer:IStorageContainer
{
   protected StorageBaseList InnerStorageBaseList;
   public StorageBase GetInnerStorage()
   {
      return InnerStorageBaseList;
   }
   public StorageBaseListContainer(StorageBaseList StorageBaseList= null)
   {
      Init(StorageBaseList);
   }
   public StorageBaseListContainer()
   {
      Init();
   }
   public void Init(StorageBase StorageBaseDictionary = null)
   {
      if (StorageBaseDictionary == null)
      {
         InnerStorageBaseList = new StorageBaseList();
      }
      else
      {
         InnerStorageBaseList = (StorageBaseList)StorageBaseDictionary;  
      }
   }
}
public class StorageBaseDictionaryContainer:IStorageContainer
{
   protected StorageBaseDictionary InnerStorageBaseDictionary;
   public StorageBase GetInnerStorage()
   {
      return InnerStorageBaseDictionary;
   }
   public StorageBaseDictionaryContainer(StorageBaseDictionary StorageBaseDictionary = null)
   {
      Init(StorageBaseDictionary);
   }

   public StorageBaseDictionaryContainer()
   {
      Init();
   }

   public void Init(StorageBase StorageBaseDictionary = null)
   {
      if (StorageBaseDictionary == null)
      {
         InnerStorageBaseDictionary = new StorageBaseDictionary();
      }
      else
      {
         InnerStorageBaseDictionary = (StorageBaseDictionary)StorageBaseDictionary;  
      }
   }
}

public class StorageInt:StorageBaseStringContainer
{
   public StorageInt():base()
   {
      Value = 0;
   }
   public StorageInt(int num = 0):base()
   {
      Value = num;
   }
   public int Value
   {
      get
      {
         string str = InnerStorageBaseString.GetValue();
         if (string.IsNullOrEmpty(str))
            return 0;
         if (int.TryParse(str, out int result))
            return result;
         UnityEngine.Debug.LogWarning($"StorageInt: 无法解析 '{str}', 返回默认值0");
         return 0;
      }
      set
      {
         InnerStorageBaseString.SetValue(value.ToString());
      }
   }
}

public class StorageFloat:StorageBaseStringContainer
{
   public StorageFloat():base()
   {
      Value = 0;
   }
   public StorageFloat(float num = 0f):base()
   {
      Value = num;
   }
   public float Value
   {
      get
      {
         string str = InnerStorageBaseString.GetValue();
         if (string.IsNullOrEmpty(str))
            return 0f;
         if (float.TryParse(str, out float result))
            return result;
         UnityEngine.Debug.LogWarning($"StorageFloat: 无法解析 '{str}', 返回默认值0");
         return 0f;
      }
      set
      {
         InnerStorageBaseString.SetValue(value.ToString());
      }
   }
}
public class StorageLong:StorageBaseStringContainer
{
   public StorageLong():base()
   {
      Value = 0;
   }
   public StorageLong(long num = 0):base()
   {
      Value = num;
   }
   public long Value
   {
      get
      {
         string str = InnerStorageBaseString.GetValue();
         if (string.IsNullOrEmpty(str))
            return 0;
         if (long.TryParse(str, out long result))
            return result;
         UnityEngine.Debug.LogWarning($"StorageLong: 无法解析 '{str}', 返回默认值0");
         return 0;
      }
      set
      {
         InnerStorageBaseString.SetValue(value.ToString());
      }
   }
}
public class StorageString:StorageBaseStringContainer
{
   public StorageString():base()
   {
      Value = string.Empty;
   }
   public StorageString(string str = ""):base()
   {
      Value = str;
   }
   public string Value
   {
      get
      {
         return InnerStorageBaseString.GetValue();
      }
      set
      {
         InnerStorageBaseString.SetValue(value);
      }
   }
}

public class StorageDictionary<T1, T2> : StorageBaseDictionaryContainer ,IEnumerable<KeyValuePair<T1,T2>>
   where T1:StorageBaseStringContainer, new() 
   where T2:IStorageContainer, new()
{
   public StorageDictionary(StorageBaseDictionary StorageBaseDictionary = null) : base(StorageBaseDictionary) { }
   public StorageDictionary() : base() { }
   private Dictionary<StorageBase, object> ObjectPool = new Dictionary<StorageBase, object>();
   public int Count => InnerStorageBaseDictionary.Count;
   private StringBuilder _sb;
   StringBuilder sb
   {
      get
      {
         if (_sb == null)
            _sb = new StringBuilder();
         return _sb;
      }
   }
   public void Clear()
   {
      InnerStorageBaseDictionary.Clear();
      ObjectPool.Clear();
   }
   public void Add(T1 key,T2 value)
   {
      sb.Clear();
      key.GetInnerStorage().Json(sb);
      var strKey = sb.ToString();
      if (ContainsKey(strKey))
         return;
      InnerStorageBaseDictionary.Add(strKey,value.GetInnerStorage());
   }

   public void Remove(T1 key)
   {
      sb.Clear();
      key.GetInnerStorage().Json(sb);
      var strKey = sb.ToString();
      InnerStorageBaseDictionary.Remove(strKey);
   }
   public bool ContainsKey(string key)
   {
      return InnerStorageBaseDictionary.ContainsKey(key);
   }
   
   public List<T1> GetKeys()
   {
      var keyValues = InnerStorageBaseDictionary.GetKeys();
      var keyList = new List<T1>();
      foreach (var keyValueStr in keyValues)
      {
         var keyValue = StorageManager.FromJson(keyValueStr,0,out var endIndex);
         if (!ObjectPool.ContainsKey(keyValue))
         {
            var key = new T1();
            key.Init(keyValue);
            ObjectPool.Add(keyValue,key);
         }
         keyList.Add((T1)ObjectPool[keyValue]);
      }
      return keyList;
   }

   public IEnumerator<KeyValuePair<T1,T2>> GetEnumerator()
   {
      foreach (var pair in InnerStorageBaseDictionary)
      {
         var keyValue = StorageManager.FromJson(pair.Key,0,out var endIndex);
         if (!ObjectPool.ContainsKey(keyValue))
         {
            var key = new T1();
            key.Init(keyValue);
            ObjectPool.Add(keyValue,key);
         }
         var valueValue = pair.Value;
         if (valueValue != null)
         {
            if (!ObjectPool.ContainsKey(valueValue))
            {
               var value = new T2();
               value.Init(valueValue);
               ObjectPool.Add(valueValue,value);
            }
         }

         var newPair = new KeyValuePair<T1, T2>((T1)ObjectPool[keyValue],
            valueValue != null ? (T2)ObjectPool[valueValue] : default);
         yield return newPair;
      }
   }
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
   
   public T2 this[T1 key]
   {
      get
      {
         sb.Clear();
         key.GetInnerStorage().Json(sb);
         var strKey = sb.ToString();
         var storageValue = InnerStorageBaseDictionary.GetValue(strKey);
         if (storageValue == null)
            return default;
         if (!ObjectPool.ContainsKey(storageValue))
         {
            var sT2 = new T2();
            sT2.Init(storageValue);
            ObjectPool.Add(storageValue,sT2);
         }
         return (T2)ObjectPool[storageValue];
      }
      set
      {
         sb.Clear();
         key.GetInnerStorage().Json(sb);
         var strKey = sb.ToString();
         var curStorageValue = value.GetInnerStorage();
         InnerStorageBaseDictionary.SetValue(strKey,curStorageValue);
      }
   }
}
public class StorageList<T> : StorageBaseListContainer,IEnumerable<T>
   where T:IStorageContainer,new()
{
   public StorageList(StorageBaseList StorageBaseList = null) : base(StorageBaseList) { }
   public StorageList() : base() { }
   private Dictionary<StorageBase, object> ObjectPool = new Dictionary<StorageBase, object>();
   public int Count => InnerStorageBaseList.Count;

   public void Add(T value)
   {
      var curStorageValue = value.GetInnerStorage();
      InnerStorageBaseList.Add(curStorageValue);
   }
   public void RemoveAt(int index)
   {
      var curStorageValue = InnerStorageBaseList.GetValue(index);
      if (curStorageValue == null)
         return;
      InnerStorageBaseList.RemoveAt(index);
   }

   public void Clear()
   {
      InnerStorageBaseList.Clear();
      ObjectPool.Clear();
   }
   public IEnumerator<T> GetEnumerator()
   {
      foreach (var item in InnerStorageBaseList)
      {
         var valueValue = item;
         if (valueValue != null)
         {
            if (!ObjectPool.ContainsKey(valueValue))
            {
               var value = new T();
               value.Init(valueValue);
               ObjectPool.Add(valueValue,value);
            }
         }
         yield return valueValue != null ? (T)ObjectPool[valueValue]:default;
      }
   }
   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }
   public T this[int index]
   {
      get
      {
         var storageValue = InnerStorageBaseList.GetValue(index);
         if (storageValue == null)
            return default;
         if (!ObjectPool.ContainsKey(storageValue))
         {
            var sT = new T();
            sT.Init(storageValue);
            ObjectPool.Add(storageValue,sT);
         }
         return (T)ObjectPool[storageValue];
      }
      set
      {
         if (index < 0 || index > Count)
            return;
         var curStorageValue = value.GetInnerStorage();
         InnerStorageBaseList.SetValue(index,curStorageValue);
      }
   }
}
public class StorageClass:StorageBaseDictionaryContainer
{
   private Dictionary<StorageBase, object> ObjectPool = new Dictionary<StorageBase, object>();
   public T GetValue<T>(string key)
      where T : IStorageContainer, new()
   {
      T valueT = default;
      if (InnerStorageBaseDictionary.GetValue(key) == null)
      {
         valueT = new T();
         valueT.Init();
         var value = valueT.GetInnerStorage();
         InnerStorageBaseDictionary.Add(key,value);
         if (!ObjectPool.TryAdd(value, valueT))
         {
            ObjectPool[value] = valueT;
         }
      }
      else
      {
         var value = InnerStorageBaseDictionary.GetValue(key);
         if (!ObjectPool.ContainsKey(value))
         {
            valueT = new T();
            valueT.Init(value);
            ObjectPool.Add(value,valueT);
         }
         else
         {
            valueT = (T)ObjectPool[value];
         }
      }
      return valueT;
   }

   public void SetValue(string key, IStorageContainer value)
   {
      if (InnerStorageBaseDictionary.GetValue(key) == null)
      {
         InnerStorageBaseDictionary.Add(key,value.GetInnerStorage());
      }
      else
      {
         InnerStorageBaseDictionary.SetValue(key,value.GetInnerStorage());
      }
   }
}

