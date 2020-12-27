using GTANetworkAPI;
using MySqlConnector;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace iTeffa.Settings
{
    public static class Connect
    {
        private static readonly Config config = new Config("MySQL");
        private static readonly Nlogs Log = new Nlogs("MySQL");
        private static string Connection = null;
        public static bool Debug = false;

        public static void Init()
        {
            if (Connection is string) return;
            Connection =
                $"Host={config.TryGet<string>("Server", "localhost")};" +
                $"Port={config.TryGet<string>("Port", 3306)};" +
                $"User={config.TryGet<string>("User", "root")};" +
                $"Password={config.TryGet<string>("Password", "@iTeffa2021")};" +
                $"Database={config.TryGet<string>("DataBase", "iteffa")};" +
                $"{config.TryGet<string>("SSL", "SslMode=None;")}";
        }

        public static void Query(MySqlCommand command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
                using MySqlConnection connection = new MySqlConnection(Connection);
                connection.Open();
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (Exception e) { Log.Write(e.ToString(), Nlogs.Type.Error); }
        }

        public static void Query(string command)
        {
            using MySqlCommand cmd = new MySqlCommand(command);
            Query(cmd);
        }

        public static async Task QueryAsync(MySqlCommand command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
                using MySqlConnection connection = new MySqlConnection(Connection);
                await connection.OpenAsync();
                command.Connection = connection;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e) { Log.Write(e.ToString(), Nlogs.Type.Error); }
        }
        public static async Task QueryAsync(string command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command);
                using MySqlConnection connection = new MySqlConnection(Connection);
                await connection.OpenAsync();
                using MySqlCommand cmd = new MySqlCommand
                {
                    Connection = connection,
                    CommandText = command
                };
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e) { Log.Write(e.ToString(), Nlogs.Type.Error); }
        }
        public static DataTable QueryRead(MySqlCommand command)
        {
            if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
            using MySqlConnection connection = new MySqlConnection(Connection);
            connection.Open();
            command.Connection = connection;
            DbDataReader reader = command.ExecuteReader();
            DataTable result = new DataTable();
            result.Load(reader);

            return result;
        }
        public static DataTable QueryRead(string command)
        {
            using MySqlCommand cmd = new MySqlCommand(command);
            return QueryRead(cmd);
        }
        public static async Task<DataTable> QueryReadAsync(MySqlCommand command)
        {
            if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
            using MySqlConnection connection = new MySqlConnection(Connection);
            await connection.OpenAsync();
            command.Connection = connection;
            DbDataReader reader = await command.ExecuteReaderAsync();
            DataTable result = new DataTable();
            result.Load(reader);
            return result;
        }
        public static async Task<DataTable> QueryReadAsync(string command)
        {
            using MySqlCommand cmd = new MySqlCommand(command);
            return await QueryReadAsync(cmd);
        }
        public static string ConvertTime(DateTime DateTime)
        {
            return DateTime.ToString("s");
        }
    }
}
