using System;
using System.Data.SQLite;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1
{
    public partial class WorkflowDialog : Form
    {
        private string selectedFileConfigPath;   // <-- path file DB yang dipilih
        private readonly IServiceProvider _serviceProvider;

        // Constructor for dependency injection
        public WorkflowDialog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        // Legacy constructor for backward compatibility
        public WorkflowDialog() : this(null)
        {
        }

        private void chooseFile_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Title = "Pilih file config";
                openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFile.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFile.DefaultExt = "json";
                openFile.AddExtension = true;

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (!openFile.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Silakan pilih file dengan ekstensi .json", "Format salah",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    selectedFileConfigPath = openFile.FileName;
                    dungeonLabel1.Text = selectedFileConfigPath;
                }
            }
        }

        private void dungeonLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(selectedFileConfigPath ?? "Belum ada file dipilih");
        }

        private void savePathBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFileConfigPath))
            {
                MessageBox.Show("Belum ada file database dipilih");
                return;
            }

            // Save the selected config file path to application settings
            try
            {
                // Get the settings service from DI
                var settingsService = _serviceProvider?.GetRequiredService<ISettingsService>();
                if (settingsService != null)
                {
                    // Save the config file path with key "workflow" and subkey "config_path"
                    settingsService.SetSetting("workflow", "config_path", selectedFileConfigPath);
                    MessageBox.Show("Path konfigurasi berhasil disimpan.", "Sukses", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine($"[Workflow] Config path saved: {selectedFileConfigPath}");
                }
                else
                {
                    MessageBox.Show("Layanan pengaturan tidak tersedia.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("[Workflow] Settings service not available, cannot save config path");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan path konfigurasi: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"[Workflow] Error saving config path: {ex.Message}");
            }
        }
        
    }
}