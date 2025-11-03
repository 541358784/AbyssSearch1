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
}
public class StorageBaseDictionary:StorageBase,IEnumerable<KeyValuePair<string,StorageBase>>
{
   private Dictionary<string, StorageBase> InnerDictionary = new Dictionary<string, StorageBase>();
   protected override void BuildJson()
   {
      StringBuilder sb = new StringBuilder();
      sb.Append('{');
      foreach (var pair in InnerDictionary)
      {
         sb.Append(pair.Key);
         sb.Append('=');
         pair.Value?.Json(sb);
         sb.Append(',');
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
      if (jsonStr[index] != '{')
         throw new Exception("字典解析错误,首位不为{");
      index++;//跳前尖括号
      while (jsonStr[index] != '}')
      {
         StringBuilder sb = new StringBuilder();
         while (jsonStr[index] != '=')
         {
            sb.Append(jsonStr[index]);
            index++;
         }
         var key = sb.ToString();
         index++;//跳等号
         var value = StorageManager.FromJson(jsonStr, index, out index);
         value.Parent = this;
         if (jsonStr[index] != ',')
            throw new Exception("字典解析错误,间隔位不为,");
         index++;//跳逗号
         InnerDictionary.Add(key,value);
      }
      index++;//跳后尖括号
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
      foreach (var item in InnerList)
      {
         item?.Json(sb);
         sb.Append(',');
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
      if (index >= 0 && index <= InnerList.Count)
      {
         InnerList.Insert(index,value);
         value.Parent = this;
         IsDirty = true;
      }
   }
   public void SetValue(int index,StorageBase value)
   {
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
      if (jsonStr[index] != '[')
         throw new Exception("列表解析错误,首位不为[");
      index++;//跳前尖括号
      while (jsonStr[index] != ']')
      {
         var value = StorageManager.FromJson(jsonStr, index, out index);
         value.Parent = this;
         if (jsonStr[index] != ',')
            throw new Exception("列表解析错误,间隔位不为,");
         index++;//跳逗号
         InnerList.Add(value);
      }
      index++;//跳后尖括号
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
      sb.Append('\"');
      sb.Append(InnerString);
      sb.Append('\"');
      json = sb.ToString();
   }

   public void SetValue(string value)
   {
      if (InnerString == value)
         return;
      InnerString = value;
      IsDirty = true;
   }

   public string GetValue()
   {
      return InnerString;
   }

   public override void FromJson(string jsonStr,int index,out int endIndex)
   {
      if (jsonStr[index] != '\"')
         throw new Exception("字符串解析错误,首位不为\"");
      index++;
      StringBuilder sb = new StringBuilder();
      while (!CheckEnd(jsonStr,index))
      {
         sb.Append(jsonStr[index]);
         index++;
      }
      index++;
      InnerString = sb.ToString();
      endIndex = index;
      IsDirty = true;
   }

   private bool CheckEnd(string jsonStr,int index)
   {
      if (jsonStr[index] != '\"')
         return false;
      var count = 0;
      index--;
      while (jsonStr[index] == '\\')
      {
         count++;
         index--;
      }
      return count % 2 == 0;
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
         return int.Parse(InnerStorageBaseString.GetValue());
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
         return float.Parse(InnerStorageBaseString.GetValue());
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
         return long.Parse(InnerStorageBaseString.GetValue());
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

