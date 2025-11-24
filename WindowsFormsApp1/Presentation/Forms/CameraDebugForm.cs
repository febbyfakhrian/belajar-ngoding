using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Core.Interfaces;

namespace WindowsFormsApp1.Presentation.Forms
{
    public partial class CameraDebugForm : Form
    {
        private string selectedImagePath = "";
        private bool isDebugMode = false;
        private readonly ISettingsService _settingsService;

        // Properties to expose the selected values
        public string SelectedImagePath => selectedImagePath;
        public bool IsDebugMode => isDebugMode;

        public CameraDebugForm(ISettingsService settingsService = null)
        {
            _settingsService = settingsService;
            InitializeComponent();
            // Initialize event handlers
            chooseFileBtn.Click += ChooseFileBtn_Click;
            okButton.Click += OkButton_Click;
            
            // Load existing settings if available
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (_settingsService != null)
                {
                    // Load debug mode setting
                    var debugModeSetting = _settingsService.GetSetting<string>("debug", "is_debug");
                    if (!string.IsNullOrEmpty(debugModeSetting))
                    {
                        isDebugMode = debugModeSetting.ToLower() == "true";
                        if (isDebugMode)
                        {
                            enableRadioButton.Checked = true;
                        }
                        else
                        {
                            disableRadioButton.Checked = true;
                        }
                    }
                    
                    // Load image path setting
                    var imagePathSetting = _settingsService.GetSetting<string>("debug", "image_path");
                    if (!string.IsNullOrEmpty(imagePathSetting))
                    {
                        selectedImagePath = imagePathSetting;
                        dungeonLabel1.Text = System.IO.Path.GetFileName(selectedImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors in loading settings
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (_settingsService != null)
                {
                    // Save debug mode setting
                    string debugMode = isDebugMode ? "true" : "false";
                    _settingsService.SetSetting("debug", "is_debug", debugMode);
                    
                    // Save image path setting
                    _settingsService.SetSetting("debug", "image_path", selectedImagePath);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors in saving settings
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            // Determine debug mode based on radio button selection
            isDebugMode = enableRadioButton.Checked;
            
            // Save settings
            SaveSettings();
            
            // Close the dialog with OK result
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ChooseFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
                Title = "Select an Image for Debug Mode"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = openFileDialog.FileName;
                dungeonLabel1.Text = System.IO.Path.GetFileName(selectedImagePath);
            }
        }

        private void savePathBtn_Click(object sender, EventArgs e)
        {
            // Determine debug mode based on radio button selection
            isDebugMode = enableRadioButton.Checked;

            // Save settings
            SaveSettings();
            
            // Show confirmation message
            MessageBox.Show($"Debug Mode: {(isDebugMode ? "Enabled" : "Disabled")}\nImage Path: {selectedImagePath}", "Settings Saved");
        }
    }
}