using ServerExample.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;

namespace ServerExample
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestTableAttribute : Attribute
    {
        public RestTableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RestColumnAttribute : Attribute
    {
        public RestColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RestIgnoreColumnAttribute : Attribute
    {
        public RestIgnoreColumnAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RestConditionAttribute : RestColumnAttribute
    {
        public RestConditionAttribute(string name) : base(name)
        {
        }

        public string Operator { get; set; }
    }

    public class RestDatabase
    {
        private static string FILE_PATH = "data/database.db";

	    private static SQLiteConnection db = null;

	    private static void init()
	    {
		    if(db == null)
            {
                if (!File.Exists(FILE_PATH))
                {
                    Install();
                    return;
                }

                db = new SQLiteConnection("Data Source=data/database.db;Version=3;");
                db.Open();
            }
	    }

        public static void Install()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FILE_PATH));
            SQLiteConnection.CreateFile(FILE_PATH);

            init();

            string sql = "CREATE TABLE users (id INTEGER PRIMARY KEY, created_at INTEGER, updated_at INTEGER, first_name TEXT, last_name TEXT, email TEXT, status INTEGER)";
            SQLiteCommand command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
        }

        public static long DateTimeToTimestamp(DateTime value)
        {
            return (long)value.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        public static DateTime DateTimeFromTimestamp(long value)
        {
            DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dateTime.AddSeconds(value);
        }

        public static string ToQueryValue(object value)
        {
            if (value == null)
            {
                return null;
            }
                
            if (value is RestDateTime)
            {
                return DateTimeToTimestamp(((RestDateTime)value).Value).ToString();
            }
                
            if (value is Enum)
            {
                return ((int)value).ToString();
            }

            if (value is string)
            {
                return WrapString((string)value);
            }

            return value.ToString();
        }


        public static string WrapString(string str)
        {
            return "\"" + str + "\"";
        }

        public static Dictionary<string, object> Insert(string table, Dictionary<string, object> data)
        {
		    init();

		    string columns = string.Join(", ", data.Keys);
            string values = string.Join(", ", data.Values);

            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, columns, values);
            SQLiteCommand command = new SQLiteCommand(sql, db);
		    command.ExecuteNonQuery();
		    long rowId = db.LastInsertRowId;
            
            sql = string.Format("SELECT * FROM {0} WHERE id = {1}", table, rowId);
            command = new SQLiteCommand(sql, db); 
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                List<string> keys = data.Keys.ToList();
                keys.Add("id");
                return Read(reader, keys);
            }

            return null;
	    }

        public static Dictionary<string, object> Update(string table, long id, Dictionary<string, object> data)
        {
            init();

            string sets = string.Join(", ", data.Select(item => string.Format("{0} = {1}", item.Key, item.Value)));

            string sql = string.Format("UPDATE {0} SET {1} WHERE ID = {2}", table, sets, id);
            SQLiteCommand command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();

            sql = string.Format("SELECT * FROM {0} WHERE id = {1}", table, id);
            command = new SQLiteCommand(sql, db);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                List<string> keys = data.Keys.ToList();
                return Read(reader, keys);
            }

            return null;
        }

        public static void Delete(string table, long id)
        {
            init();

            string sql = string.Format("DELETE FROM {0} WHERE ID = {1}", table, id);
            SQLiteCommand command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
        }

        public static Dictionary<string, object> Select(string table, Dictionary<string, string> where = null, List<string> columns = null)
        {
            init();

            string columnsClause = columns == null ? "*" : string.Join(", ", columns);
            string whereClause = where == null ? "" : "WHERE " + string.Join(" AND ", where.Select(item =>  string.Format("{0} = {1}", item.Key, ToQueryValue(item.Value))));

            string sql = string.Format("SELECT {0} FROM {1} {2}", columnsClause, table, whereClause);
            SQLiteCommand command = new SQLiteCommand(sql, db);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return Read(reader, columns);
            }

            return null;
        }

        public static long Count(string table, List<string> where = null)
        {
            init();

            string whereClause = where == null ? "" : "WHERE " + string.Join(" AND ", where);

            string sql = string.Format("SELECT COUNT(id) as cnt FROM {0} {1}", table, whereClause);
            SQLiteCommand command = new SQLiteCommand(sql, db);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();

            return (long)reader["cnt"];
        }

        public static List<Dictionary<string, object>> Search(string table, List<string> where, long pageSize, long pageIndex, List<string> columns = null)
        {
            init();

            string columnsClause = columns == null ? "*" : string.Join(", ", columns);
            string whereClause = where.Count() == 0 ? "" : "WHERE " + string.Join(" AND ", where);
            string limitClause = pageSize.ToString();
            long offset = (pageIndex - 1) * pageSize;
            if (offset > 0)
            {
                limitClause = string.Format("{0}, {1}", pageSize, offset);
            }

            string sql = string.Format("SELECT {0} FROM {1} {2} LIMIT {3}", columnsClause, table, whereClause, limitClause);
            SQLiteCommand command = new SQLiteCommand(sql, db);
            SQLiteDataReader reader = command.ExecuteReader();

            List<Dictionary<string, object>> records = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                records.Add(Read(reader, columns));
            }

            return records;
        }

        public static Dictionary<string, object> Read(SQLiteDataReader reader, List<string> keys = null)
        {
            if (keys == null)
            {
                keys = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (string key in keys)
            {
                data[key] = reader[key] is DBNull ? null : reader[key];
            }

            return data;
        }
    }
}