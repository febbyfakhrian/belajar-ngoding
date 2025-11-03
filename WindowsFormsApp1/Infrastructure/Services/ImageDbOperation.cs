using System;
using System.Data.SQLite;
using WindowsFormsApp1.Core.Entities.Models;
using System.Collections.Generic;
using System.Data;

namespace WindowsFormsApp1.Infrastructure.Data
{
    public class ImageDbOperation
    {
        private readonly SQLiteConnection _connection;

        public ImageDbOperation(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Insert atau ignore record baru ke tabel images
        /// </summary>
        public void InsertImage(string fileName, string imageId)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(imageId))
                return;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT OR IGNORE INTO images (file_name, image_id) VALUES (@FileName, @ImageId);";
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@ImageId", imageId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Cari file name berdasarkan imageId (LIKE)
        /// </summary>
        public string FindById(string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId))
                return null;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT file_name FROM images WHERE image_id LIKE '%' || @id || '%' LIMIT 1;";
                cmd.Parameters.AddWithValue("@id", imageId);

                var result = cmd.ExecuteScalar();

                return result?.ToString();
            }
        }

        public string GetLatestImageById()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT file_name FROM images order by id DESC LIMIT 1;";

                var result = cmd.ExecuteScalar();

                return result?.ToString();
            }
        }

        /// <summary>
        /// Fetch all images from the images table
        /// </summary>
        public DataTable GetAllImages()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("File Name", typeof(string));
            dataTable.Columns.Add("Image ID", typeof(string));

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT id, file_name, image_id FROM images ORDER BY id DESC";

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
                        row["File Name"] = reader["file_name"];
                        row["Image ID"] = reader["image_id"];
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Buat tabel images kalau belum ada
        /// </summary>
        public void CreateTableIfNotExists()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS images (
                    id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    file_name  TEXT NOT NULL,
                    image_id   TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_images_imageid ON images(image_id);";
                cmd.ExecuteNonQuery();
            }
        }
    }
}