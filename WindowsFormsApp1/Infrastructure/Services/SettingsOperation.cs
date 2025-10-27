using Newtonsoft.Json;
using System;
using System.Data.SQLite;

namespace WindowsFormsApp1.Services
{
    internal class SettingsOperation
    {
        private readonly SQLiteConnection _connection;

        public SettingsOperation(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public T GetSetting<T>(string groupName, string key)
        {
            // Ensure the connection is open
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            string sql = "SELECT value FROM settings WHERE group_name = @group AND key = @key LIMIT 1";

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@group", groupName);
                cmd.Parameters.AddWithValue("@key", key);

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return default;

                string value = result.ToString();

                // otomatis konversi tipe target
                try
                {
                    // jika T adalah string, langsung kembalikan
                    if (typeof(T) == typeof(string))
                        return (T)(object)value;

                    // jika T adalah bool
                    if (typeof(T) == typeof(bool))
                        return (T)(object)(value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1");

                    // jika T adalah int atau double
                    if (typeof(T).IsPrimitive)
                        return (T)Convert.ChangeType(value, typeof(T));

                    // jika bukan tipe dasar, coba parse JSON
                    return JsonConvert.DeserializeObject<T>(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetSetting] Error convert {groupName}:{key} => {ex.Message}");
                    return default;
                }
            }
        }

        public void SetSetting(string groupName, string key, object value)
        {
            // Ensure the connection is open
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            string sql = @"
            INSERT INTO settings (group_name, key, value, updated_at)
            VALUES (@group, @key, @value, CURRENT_TIMESTAMP)
            ON CONFLICT(key) DO UPDATE SET
                value = excluded.value,
                updated_at = CURRENT_TIMESTAMP;
            ";

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@group", groupName);
                cmd.Parameters.AddWithValue("@key", key);
                cmd.Parameters.AddWithValue("@value", value?.ToString() ?? "");
                cmd.ExecuteNonQuery();
            }
        }

    }
}