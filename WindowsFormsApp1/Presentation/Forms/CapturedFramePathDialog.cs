using System;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Hardware.Grpc; // Add this using statement
using WindowsFormsApp1.Core.Interfaces; // Add this using statement

namespace WindowsFormsApp1
{
    public partial class CapturedFramePathDialog : Form
    {
        private string selectedPath;
        private readonly ISettingsService _settingsService;

        public CapturedFramePathDialog()
        {
            InitializeComponent();
        }

        // Constructor with settings service injection
        public CapturedFramePathDialog(ISettingsService settingsService) : this()
        {
            _settingsService = settingsService;
            LoadSavedFolderPath();
        }

        public string SelectedFolderPath => selectedPath;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chooseFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Pilih folder untuk menyimpan file";
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = folderDialog.SelectedPath; // Simpan ke field
                    dungeonLabel1.Text = selectedPath;        // Tampilkan di label
                }
            }
        }

        private void dungeonLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(selectedPath ?? "Belum ada folder dipilih");
        }

        private void savePathBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            {
                MessageBox.Show("Belum ada folder dipilih");
                return;
            }

            try
            {
                // Save the folder path to the settings service
                _settingsService?.SetSetting("captured_frames", "folder_path", SelectedFolderPath);

                MessageBox.Show("Folder path saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal kirim konfigurasi:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Load the saved folder path from the settings service
        /// </summary>
        private void LoadSavedFolderPath()
        {
            try
            {
                // Load the folder path from the settings service
                var savedPath = _settingsService?.GetSetting<string>("captured_frames", "folder_path");
                
                if (!string.IsNullOrWhiteSpace(savedPath))
                {
                    selectedPath = savedPath;
                    dungeonLabel1.Text = savedPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load saved folder path: {ex.Message}");
            }
        }
    }
}