using System;
using System.Data.SQLite;
using System.Windows.Forms;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1
{
    public partial class DatabaseDialog : Form
    {
        private string selectedDbPath;   // <-- path file DB yang dipilih
        public static ImageDbOperation DbHelper;
        public static SQLiteConnection DbConnection;

        public DatabaseDialog()
        {
            InitializeComponent();
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

            // Buka DB langsung dari file yang dipilih
            DbConnection = new SQLiteConnection($"Data Source={selectedDbPath};Version=3;");
            DbConnection.Open();

            DbHelper = new ImageDbOperation(DbConnection);
            DbHelper.CreateTableIfNotExists();

            MessageBox.Show($"Database siap: {selectedDbPath}");
        }
    }
}