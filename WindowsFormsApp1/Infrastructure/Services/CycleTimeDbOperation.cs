using System;
using System.Data.SQLite;
using WindowsFormsApp1.Core.Entities.Models;
using System.Data;

namespace WindowsFormsApp1.Infrastructure.Services
{
    public class CycleTimeDbOperation
    {
        private readonly SQLiteConnection _connection;

        public CycleTimeDbOperation(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Create the cycle_times table if it doesn't exist
        /// </summary>
        public void CreateTableIfNotExists()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS cycle_times (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transaction_id TEXT NOT NULL,
                    start_time DATETIME NOT NULL,
                    end_time DATETIME NOT NULL,
                    cycle_time_ms INTEGER NOT NULL,
                    raw_response TEXT,
                    pass INTEGER NOT NULL,       -- 1 = Pass, 0 = Fail
                    image_id TEXT,               -- bisa simpan nama file / UUID / foreign key
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Insert a cycle time record into the database
        /// </summary>
        public void InsertCycleTime(
            string transactionId,
            DateTime startTime,
            DateTime endTime,
            int cycleTimeMs,
            string rawResponse,
            bool pass,
            string imageId)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO cycle_times (
                        transaction_id, 
                        start_time, 
                        end_time, 
                        cycle_time_ms, 
                        raw_response, 
                        pass, 
                        image_id
                    ) VALUES (
                        @transactionId, 
                        @startTime, 
                        @endTime, 
                        @cycleTimeMs, 
                        @rawResponse, 
                        @pass, 
                        @imageId
                    );";

                cmd.Parameters.AddWithValue("@transactionId", transactionId);
                cmd.Parameters.AddWithValue("@startTime", startTime);
                cmd.Parameters.AddWithValue("@endTime", endTime);
                cmd.Parameters.AddWithValue("@cycleTimeMs", cycleTimeMs);
                cmd.Parameters.AddWithValue("@rawResponse", rawResponse ?? string.Empty);
                cmd.Parameters.AddWithValue("@pass", pass ? 1 : 0);
                cmd.Parameters.AddWithValue("@imageId", imageId ?? string.Empty);

                cmd.ExecuteNonQuery();
            }
        }

        public DataTable SummaryInspectionResult()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Metric", typeof(string));
            dataTable.Columns.Add("Value", typeof(string));

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"
                SELECT 
                    COUNT(*) AS total_inspection,
                    SUM(CASE WHEN pass = 1 THEN 1 ELSE 0 END) AS total_pass,
                    ROUND(AVG(cycle_time_ms), 2) AS average_cycle_time_ms,
                    ROUND(
                        (CAST(SUM(CASE WHEN pass = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*)) * 100, 
                        2
                    ) AS inspection_rate_percent
                FROM cycle_times;";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // ambil nilai hasil agregat
                        int totalInspection = Convert.ToInt32(reader["total_inspection"]);
                        int totalPass = Convert.ToInt32(reader["total_pass"]);
                        double avgCycleTime = reader.IsDBNull(reader.GetOrdinal("average_cycle_time_ms"))
                            ? 0 : reader.GetDouble(reader.GetOrdinal("average_cycle_time_ms"));
                        double rate = reader.IsDBNull(reader.GetOrdinal("inspection_rate_percent"))
                            ? 0 : reader.GetDouble(reader.GetOrdinal("inspection_rate_percent"));

                        // tambahkan baris vertikal
                        dataTable.Rows.Add("Total Inspection", totalInspection);
                        dataTable.Rows.Add("Total Pass", totalPass);
                        dataTable.Rows.Add("Average Cycle Time (ms)", avgCycleTime.ToString("F2"));
                        dataTable.Rows.Add("Inspection Rate (%)", rate.ToString("F2"));
                    }
                }
            }

            return dataTable;
        }


        /// <summary>
        /// Fetch all cycle times from the database
        /// </summary>
        public DataTable GetAllCycleTimes()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Transaction ID", typeof(string));
            dataTable.Columns.Add("Start Time", typeof(DateTime));
            dataTable.Columns.Add("End Time", typeof(DateTime));
            dataTable.Columns.Add("Cycle Time (ms)", typeof(int));
            dataTable.Columns.Add("Pass", typeof(bool));
            dataTable.Columns.Add("Image ID", typeof(string));
            dataTable.Columns.Add("Created At", typeof(DateTime));

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT id, transaction_id, start_time, end_time, cycle_time_ms, pass, image_id, created_at FROM cycle_times ORDER BY created_at DESC";

                // Ensure connection is open
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = dataTable.NewRow();
                        row["ID"] = reader["id"];
                        row["Transaction ID"] = reader["transaction_id"];
                        row["Start Time"] = reader["start_time"];
                        row["End Time"] = reader["end_time"];
                        row["Cycle Time (ms)"] = reader["cycle_time_ms"];
                        row["Pass"] = Convert.ToInt32(reader["pass"]) == 1;
                        row["Image ID"] = reader["image_id"];
                        row["Created At"] = reader["created_at"];
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }
    }
}