using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace iTeffa.Settings
{
    public class Config
    {
        private readonly Dictionary<string, object> configs;
        private readonly string Category;
        private readonly string DBCONN = "Data Source=iTeffa.db;Version=3;";
        public Config(string category_)
        {
            configs = new Dictionary<string, object>();
            Category = category_;

            using (SQLiteConnection connection = new SQLiteConnection())
            {
                connection.ConnectionString = DBCONN;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"CREATE TABLE IF NOT EXISTS '{Category}' " + "(Param TEXT NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Param))";
                    command.ExecuteNonQuery();
                    command.CommandText = $"SELECT * FROM '{Category}'";

                    SQLiteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();

                    table.Load(reader);

                    foreach (DataRow row in table.Rows)
                    {
                        configs.Add(row["Param"].ToString(), row["Value"]);
                        Console.WriteLine($"Loaded config: {Category} {row["Param"].ToString()} {row["Value"]}");
                    }
                }
            }
        }

        public object Set(string param, object value)
        {
            if (configs.ContainsKey(param))
            {
                configs[param] = value;
                using (SQLiteConnection connection = new SQLiteConnection())
                {
                    connection.ConnectionString = DBCONN;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"UPDATE '{Category}' SET 'Value'='{value.ToString()}' WHERE 'Param'='{param}'";
                        command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                configs.Add(param, value);
                using (SQLiteConnection connection = new SQLiteConnection())
                {
                    connection.ConnectionString = DBCONN;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"INSERT INTO '{Category}'('Param','Value') VALUES ('{param}','{value.ToString()}')";
                        command.ExecuteNonQuery();
                    }
                }
            }
            return value;
        }

        public object Get(string param)
        {
            if (configs.ContainsKey(param))
                return configs[param];
            return null;
        }

        public T TryGet<T>(string param, object _default)
        {
            if (!configs.ContainsKey(param))
            {
                Set(param, _default);
                return (T)Convert.ChangeType(configs[param], typeof(T));
            }
            else return (T)Convert.ChangeType(configs[param], typeof(T));
        }
    }
}
