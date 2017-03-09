using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ServerExample.Errors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    [DataContract(Namespace = "")]
    public class RestPersistentObject : RestObject
    {
        private static Dictionary<Type, string> tableNames = new Dictionary<Type, string>();
        private static Dictionary<Type, Dictionary<string, string>> tableMaps = new Dictionary<Type, Dictionary<string, string>>();

        [DataMember(Name = "id")]
        public long? Id { get; set; }

        [DataMember(Name = "createdAt")]
        [RestColumn("created_at")]
        public RestDateTime CreatedAt { get; set; }

        [DataMember(Name = "updatedAt")]
        [RestColumn("updated_at")]
        public RestDateTime UpdatedAt { get; set; }

        public RestPersistentObject()
        {
        }

        public RestPersistentObject(Dictionary<string, object> record) 
            : this()
        {
            Populate(record);
        }

        public RestPersistentObject(long id)
            : this()
        {
            Get(id);
        }

        protected virtual void SetDefaults()
        {
            CreatedAt = new RestDateTime(DateTime.UtcNow);
            UpdatedAt = CreatedAt;
        }

        public T Add<T>() where T : RestPersistentObject
        {
            SetDefaults();

            Dictionary<string, object> record = RestDatabase.Insert(GetTableName(), ToValues());
            Populate(record);

            return (T)this;
        }

        protected RestPersistentObject Get(long id)
        {
            Dictionary<string, object> record = RestDatabase.Select(GetTableName(), new Dictionary<string, string> { { "id", id.ToString() } });
            if (record == null)
                throw new RestApplicationException(RestApplicationException.OBJECT_NOT_FOUND, GetType().Name, id.ToString());

            Populate(record);
            return this;
        }

        public T Update<T>(T objectToUpdate) where T : RestPersistentObject
        {
            objectToUpdate.UpdatedAt = new RestDateTime(DateTime.Now);

            Dictionary<string, object> record = RestDatabase.Update(GetTableName(), Id.Value, objectToUpdate.ToValues());

            Populate(record);
            return (T)this;
        }

        public void Delete()
        {
            RestDatabase.Delete(GetTableName(), Id.Value);
        }

        protected string GetTableName()
        {
            Type type = GetType();
            if (!tableNames.ContainsKey(type))
            {
                RestTableAttribute tableAttribute = type.GetCustomAttribute<RestTableAttribute>();
                if (tableAttribute == null)
                {
                    return null;
                }

                tableNames[type] = tableAttribute.Name;
            }

            return tableNames[type];
        }

        private Dictionary<string, string> GetTableMap()
        {
            Type type = GetType();
            if (!tableMaps.ContainsKey(type))
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                string value;
                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (property.GetCustomAttribute<RestIgnoreColumnAttribute>() != null)
                    {
                        continue;
                    }

                    RestColumnAttribute columnAttribute = property.GetCustomAttribute<RestColumnAttribute>();
                    if (columnAttribute != null)
                    {
                        value = columnAttribute.Name;
                    }
                    else
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
                    }

                    map[property.Name] = value;
                }

                tableMaps[type] = map;
            }

            return tableMaps[type];
        }

        protected Dictionary<string, object> ToValues()
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            Dictionary<string, string> map = GetTableMap();

            Type type = GetType();
            foreach (string key in map.Keys)
            {
                PropertyInfo property = type.GetProperty(key);
                object value = RestDatabase.ToQueryValue(property.GetValue(this));
                if (value != null)
                {
                    values[map[key]] = value;
                }
            }

            return values;
        }

        public void Populate(Dictionary<string, object> record)
        {
            Dictionary<string, string> map = GetTableMap();

            Type type = GetType();

            string column;
            object value;
            Type propertyType;
            foreach (string key in map.Keys)
            {
                PropertyInfo property = type.GetProperty(key);
                column = map[key];
                if (!record.ContainsKey(column))
                {
                    continue;
                }

                value = record[column];
                propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }
                if (propertyType == typeof(RestDateTime))
                {
                    value = new RestDateTime(RestDatabase.DateTimeFromTimestamp((long)value));
                }
                else if (propertyType.IsEnum)
                {
                    value = Enum.Parse(propertyType, value.ToString());
                }
                 
                property.SetValue(this, value);
            }
        }
    }
}