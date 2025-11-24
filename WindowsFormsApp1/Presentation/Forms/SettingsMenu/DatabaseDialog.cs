using System;
using System.Data.SQLite;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace WindowsFormsApp1
{
    public partial class DatabaseDialog : Form
    {
        private string selectedDbPath;   // <-- path file DB yang dipilih
        private readonly ImageDbOperation _dbHelper;
        private readonly SQLiteConnection _dbConnection;
        private readonly IServiceProvider _serviceProvider;

        // Constructor for dependency injection
        public DatabaseDialog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        // Legacy constructor for backward compatibility
        public DatabaseDialog() : this(null)
        {
        }

        public string SelectedDbPath => selectedDbPath;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chooseFile_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Title = "Pilih file database";
                openFile.Filter = "SQLite files (*.sqlite;*.sqlite3;*.db)|*.sqlite;*.sqlite3;*.db|All files (*.*)|*.*";
                openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    selectedDbPath = openFile.FileName;   // full path file
                    dungeonLabel1.Text = selectedDbPath;  // tampilkan
                }
            }
        }

        private void dungeonLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(selectedDbPath ?? "Belum ada file dipilih");
        }

        private void savePathBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedDbPath))
            {
                MessageBox.Show("Belum ada file database dipilih");
                return;
            }

            try
            {
                // Use the injected service provider to get database services
                if (_serviceProvider != null)
                {
                    var connection = _serviceProvider.GetRequiredService<SQLiteConnection>();
                    var dbHelper = _serviceProvider.GetRequiredService<ImageDbOperation>();
                    
                    // Update the static references for backward compatibility
                    // This is a temporary solution until all references are updated
                    DatabaseDialog.DbHelper = dbHelper;
                    DatabaseDialog.DbConnection = connection;
                    
                    // Configure the connection with the selected database path
                    var connectionString = $"Data Source={selectedDbPath};Version=3;";
                    connection.ConnectionString = connectionString;
                    
                    // Open the connection if it's not already open
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    this.Close();
                }
                else
                {
                    // Fallback to the original implementation for backward compatibility
                    DatabaseDialog.DbConnection = new SQLiteConnection($"Data Source={selectedDbPath};Version=3;");
                    DatabaseDialog.DbConnection.Open();

                    DatabaseDialog.DbHelper = new ImageDbOperation(DatabaseDialog.DbConnection);
                }

                MessageBox.Show($"Database siap: {selectedDbPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal membuka database: {ex.Message}");
            }
        }
        
        // Static properties for backward compatibility
        // These should be removed once all references are updated to use DI
        public static ImageDbOperation DbHelper;
        public static SQLiteConnection DbConnection;
    }
}