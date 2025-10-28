﻿﻿using System;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Hardware.Grpc; // Add this using statement

namespace WindowsFormsApp1
{
    public partial class PathLogDialog : Form
    {
        private string selectedPath;
        private readonly GrpcService _grpc = new GrpcService();

        public PathLogDialog()
        {
            InitializeComponent();
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

        private async void savePathBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("test");
            if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            {
                MessageBox.Show("Belum ada folder dipilih");
                return;
            }

            try
            {
                var resp = await _grpc.UpdateConfigAsync(SelectedFolderPath);
                MessageBox.Show($"Server answered: {resp.Status} - {resp.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal kirim konfigurasi:\n{ex.Message}");
            }
        }
    }
}