using System.Collections.Generic;

namespace SRF.Helpers
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class PropertyReference
    {
        private readonly PropertyInfo _property;
        private readonly object _target;
        private bool IsDynamic;
        private string DynamicName;
        private Type DynamicType;
        private Action<object> DynamicSetFunc;
        private Func<object> DynamicGetFunc;
        private List<Attribute> DynamicAttributes;
        public PropertyReference(object target, PropertyInfo property)
        {
            SRDebugUtil.AssertNotNull(target);

            _target = target;
            _property = property;
        }

        public PropertyReference(string name,Type type, Action<object> setFunc, Func<object> getFunc,List<Attribute> attributes = null)
        {
            IsDynamic = true;
            DynamicName = name;
            DynamicType = type;
            DynamicSetFunc = setFunc;
            DynamicGetFunc = getFunc;
            DynamicAttributes = attributes;
        }

        public string PropertyName
        {
            get
            {
                if (IsDynamic)
                    return DynamicName;
                return _property.Name;
            }
        }

        public Type PropertyType
        {
            get
            {
                if (IsDynamic)
                    return DynamicType;
                return _property.PropertyType;
            }
        }

        public bool CanRead
        {
            get
            {
#if NETFX_CORE
				return _property.GetMethod != null && _property.GetMethod.IsPublic;
#else
                if (IsDynamic)
                    return DynamicGetFunc != null;
                return _property.GetGetMethod() != null;
#endif
            }
        }

        public bool CanWrite
        {
            get
            {
#if NETFX_CORE
				return _property.SetMethod != null && _property.SetMethod.IsPublic;
#else
                if (IsDynamic)
                    return DynamicSetFunc != null;
                return _property.GetSetMethod() != null;
#endif
            }
        }

        public object GetValue()
        {
            if (IsDynamic)
            {
                if (DynamicGetFunc == null)
                {
                    return null;
                }
                return DynamicGetFunc.Invoke();
            }
            if (_property.CanRead)
            {
                return SRReflection.GetPropertyValue(_target, _property);
            }

            return null;
        }

        public void SetValue(object value)
        {
            if (IsDynamic)
            {
                if (DynamicSetFunc == null)
                {
                    return;
                }
                DynamicSetFunc.Invoke(value);
                return;
            }
            if (_property.CanWrite)
            {
                SRReflection.SetPropertyValue(_target, _property, value);
            }
            else
            {
                throw new InvalidOperationException("Can not write to property");
            }
        }

        public T GetAttribute<T>() where T : Attribute
        {
            if (IsDynamic)
            {
                if (DynamicAttributes == null)
                {
                    return null;
                }
                var dynamicAttribute = DynamicAttributes.Find(a => a is T);
                return dynamicAttribute as T;
            }
            var attributes = _property.GetCustomAttributes(typeof (T), true).FirstOrDefault();

            return attributes as T;
        }
    }
}
