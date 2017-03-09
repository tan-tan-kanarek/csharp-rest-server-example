using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace ServerExample.Model
{
    public abstract class RestFilter<T> : RestObject where T : RestPersistentObject, new()
    {
        private static Dictionary<Type, string> tableNames = new Dictionary<Type, string>();
        private static Dictionary<Type, Dictionary<string, RestConditionAttribute>> conditionsMaps = new Dictionary<Type, Dictionary<string, RestConditionAttribute>>();

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

        private Dictionary<string, RestConditionAttribute> GetConditionsMap()
        {
            Type type = GetType();
            if (!conditionsMaps.ContainsKey(type))
            {
                Dictionary<string, RestConditionAttribute> map = new Dictionary<string, RestConditionAttribute>();
                foreach (PropertyInfo property in type.GetProperties())
                {
                    RestConditionAttribute conditionAttribute = property.GetCustomAttribute<RestConditionAttribute>();
                    if (conditionAttribute != null)
                    {
                        map[property.Name] = conditionAttribute;
                    }
                }

                conditionsMaps[type] = map;
            }

            return conditionsMaps[type];
        }


        protected virtual List<string> BuildQuery()
        {
            List<string> conditions = new List<string>();
            Dictionary<string, RestConditionAttribute> map = GetConditionsMap();

            Type type = GetType();
            foreach (string key in map.Keys)
            {
                RestConditionAttribute conditionAttribute = map[key];
                PropertyInfo property = type.GetProperty(key);
                object value = RestDatabase.ToQueryValue(property.GetValue(this));
                if (value != null)
                {
                    string condition = string.Format("{0} {1} {2}", conditionAttribute.Name, conditionAttribute.Operator, value);
                    conditions.Add(condition);
                }
            }

            return conditions;
        }

        public TList Search<TList>(RestPager pager) where TList : RestObjectsList<T>, new()
        {
		    List<string> where = BuildQuery();
		
		    List<Dictionary<string, object>> records = RestDatabase.Search(GetTableName(), where, pager.PageSize, pager.PageIndex);

            TList list = new TList();
		    foreach(Dictionary<string, object> record in records)
		    {
                T item = new T();
                item.Populate(record);
                list.Objects.Add(item);
		    }
		
		    list.TotalCount = list.Objects.Count();
		    if(list.TotalCount == pager.PageSize)
		    {
			    list.TotalCount = RestDatabase.Count(GetTableName(), where);
		    }
		
		    return list;
        }
    }
}