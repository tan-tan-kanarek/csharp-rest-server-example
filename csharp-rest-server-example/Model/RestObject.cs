using Newtonsoft.Json.Linq;
using ServerExample.Errors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public class RestObject : IRestObject
    {
        private static Dictionary<Type, Dictionary<string, string>> jsonMaps = new Dictionary<Type, Dictionary<string, string>>();

        [DataMember(Name = "objectType")]
        [RestIgnoreColumn]
        public string ObjectType
        {
            get
            {
                return getObjectType();
            }

            set { }
        }

        public RestObject()
        {
        }

        private Dictionary<string, string> GetJsonMap()
        {
            Type type = GetType();
            if (!jsonMaps.ContainsKey(type))
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                string value;
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                {
                    DataMemberAttribute dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                    if (dataMemberAttribute != null)
                    {
                        value = dataMemberAttribute.Name;
                    }
                    else
                    {
                        continue;
                    }

                    map[property.Name] = value;
                }

                jsonMaps[type] = map;
            }

            return jsonMaps[type];
        }

        private object ToJson(object value)
        {
            if (value is RestDateTime)
            {
                return RestDatabase.DateTimeToTimestamp(((RestDateTime)value).Value);
            }
            
            if (value is Enum)
            {
                return (int)value;
            }
            
            if (value is IRestObject)
            {
                return ((IRestObject)value).ToJson();
            }

            if (value is IList)
            {
                List<object> list = new List<object>();
                foreach (object item in (IList)value)
                {
                    list.Add(ToJson(item));
                }
                return list;
            }

            if (value is IDictionary)
            {
                Dictionary<string, object> map = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> item in (IDictionary)value)
                {
                    map.Add(item.Key, ToJson(item.Value));
                }
                return map;
            }

            return value;
        }

        protected virtual string getObjectType()
        {
            return GetType().Name;
        }

        public static object FromJson(Type type, JToken jToken)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(type);
                return FromJson(underlyingType, jToken);
            }
            else if (type == typeof(RestDateTime))
            {
                return new RestDateTime(RestDatabase.DateTimeFromTimestamp((long)jToken));
            }
            else if (jToken is JObject && type.IsSubclassOf(typeof(RestObject)))
            {
                RestObject obj = (RestObject)Activator.CreateInstance(type);
                obj.FromJson((JObject)jToken);
                return obj;
            }
            else if(type.IsEnum)
            {
                return Enum.ToObject(type, (int) jToken);
            }
            else
            {
                object obj = jToken.ToObject(type);
                return obj;
            }
        }

        public void FromJson(JObject jObject)
        {
            Dictionary<string, string> map = GetJsonMap();

            Type type = GetType();
            foreach (string key in map.Keys)
            {
                string name = map[key];
                if (jObject[name] == null || jObject[name].Type == JTokenType.Null)
                {
                    continue;
                }

                PropertyInfo property = type.GetProperty(key);
                property.SetValue(this, FromJson(property.PropertyType, jObject[name]));
            }
        }

        public Dictionary<string, object> ToJson()
        {
            Dictionary<string, object> json = new Dictionary<string, object>();
            Dictionary<string, string> map = GetJsonMap();

            Type type = GetType();
            foreach (string key in map.Keys)
            {
                PropertyInfo property = type.GetProperty(key);
                object value = property.GetValue(this);
                if (value != null)
                {
                    json[map[key]] = ToJson(value);
                }
            }

            return json;
        }
    }
}