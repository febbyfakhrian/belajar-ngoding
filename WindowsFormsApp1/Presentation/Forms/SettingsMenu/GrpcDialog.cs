using System;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Hardware.Grpc;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;
using System.Diagnostics; // Added for logging
using System.IO;

namespace WindowsFormsApp1
{
    public partial class GRPCDialog : Form
    {
        private GrpcService _grpcService = new GrpcService();
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService; // Made readonly

        // Constructor with dependency injection for both service provider and settings service
        public GRPCDialog(IServiceProvider serviceProvider, ISettingsService settingsService = null)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService ?? GetSettingsServiceFromProvider();
            
            // Initialize the gRPC service with the settings service
            _grpcService = new GrpcService(_settingsService);
            
            InitializeComponent();
            
            // Load saved settings if they exist
            LoadSettings();
        }

        // Legacy constructor for backward compatibility
        public GRPCDialog() : this(null, null)
        {
            // Initialize the gRPC service with default settings
            _grpcService = new GrpcService();
        }

        private ISettingsService GetSettingsServiceFromProvider()
        {
            // If we have a service provider, use it to get the service
            if (_serviceProvider != null)
            {
                try
                {
                    return _serviceProvider.GetRequiredService<ISettingsService>();
                }
                catch (Exception ex)
                {
                    // Log the error for debugging purposes
                    Debug.WriteLine($"Error getting settings service from provider: {ex.Message}");
                    Console.WriteLine($"Error getting settings service from provider: {ex.Message}");
                }
            }
            
            // Fallback to null for backward compatibility
            return null;
        }

        // Mock settings service for testing connections with a specific URL
        private class MockGrpcSettingsService : ISettingsService
        {
            private readonly string _grpcUrl;
            
            public MockGrpcSettingsService(string grpcUrl)
            {
                _grpcUrl = grpcUrl;
            }
            
            public T GetSetting<T>(string groupName, string key)
            {
                if (groupName == "grpc" && key == "server_url" && typeof(T) == typeof(string))
                {
                    return (T)(object)_grpcUrl;
                }
                return default(T);
            }
            
            public void SetSetting(string groupName, string key, object value)
            {
                // No-op for mock service
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (_settingsService != null)
                {
                    // Load saved GRPC URL
                    string savedUrl = _settingsService.GetSetting<string>("grpc", "server_url");
                    if (!string.IsNullOrEmpty(savedUrl))
                    {
                        urlHostGrpcTextBox.Text = savedUrl;
                    }
                    else
                    {
                        // Set default value if no saved settings
                        urlHostGrpcTextBox.Text = "localhost:50052";
                    }
                }
                else
                {
                    // Set default value if no settings service
                    urlHostGrpcTextBox.Text = "localhost:50052";
                    ShowSettingsServiceUnavailableMessage("loading");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                Console.WriteLine($"Error loading settings: {ex.Message}");
                // Set default value if error loading settings
                urlHostGrpcTextBox.Text = "localhost:50052";
            }
        }

        // Removed the GetSettingsService method as we now inject the service directly

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void testConnectionBtn_Click(object sender, EventArgs e)
        {
            // Disable button during testing to prevent multiple clicks
            testConnectionBtn.Enabled = false;
            
            try
            {
                // Get the URL from the textbox
                string grpcUrl = urlHostGrpcTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(grpcUrl))
                {
                    MessageBox.Show("Please enter a valid GRPC URL.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Create a temporary GRPC service for testing connection
                var mockSettings = new MockGrpcSettingsService(grpcUrl);
                var testService = new GrpcService(mockSettings);
                
                try
                {
                    // Test the connection
                    bool isConnected = await testService.StartAsync();
                    
                    if (isConnected)
                    {
                        saveBtn.Enabled = true;
                        updateConfigBtn.Enabled = true;
                        MessageBox.Show($"Successfully connected to GRPC server at {grpcUrl}", "Connection Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        saveBtn.Enabled = false;
                        updateConfigBtn.Enabled = false;
                        MessageBox.Show($"Failed to connect to GRPC server at {grpcUrl}", "Connection Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    // Clean up the test service
                    testService.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", "Connection Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button
                testConnectionBtn.Enabled = true;
            }
        }

        private async void saveBtn_Click(object sender, EventArgs e)
        {
            // Disable button during save operation to prevent multiple clicks
            saveBtn.Enabled = false;
            testConnectionBtn.Enabled = false;
            
            try
            {
                // Get the URL from the textbox
                string grpcUrl = urlHostGrpcTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(grpcUrl))
                {
                    MessageBox.Show("Please enter a valid GRPC URL.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Check if settings service is available
                if (_settingsService == null)
                {
                    ShowSettingsServiceUnavailableMessage("saving");
                    return;
                }
                
                // Save the GRPC URL to settings first
                _settingsService.SetSetting("grpc", "server_url", grpcUrl);
                
                // Now reconnect the main gRPC service instance used by the application
                IGrpcService mainGrpcService = null;
                if (_serviceProvider != null)
                {
                    try
                    {
                        mainGrpcService = _serviceProvider.GetRequiredService<IGrpcService>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting main gRPC service from provider: {ex.Message}");
                    }
                }
                
                if (mainGrpcService != null)
                {
                    // Stop the existing service
                    await mainGrpcService.StopAsync();
                    
                    // Update the host and restart the service
                    if (mainGrpcService is GrpcService grpcServiceInstance)
                    {
                        grpcServiceInstance.UpdateHost(grpcUrl);
                    }
                    
                    // Try to start with new settings
                    bool isConnected = await mainGrpcService.StartAsync();
                    
                    if (isConnected)
                    {
                        MessageBox.Show($"GRPC settings saved and connection established successfully.\nURL: {grpcUrl}", "Save Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"GRPC URL saved but connection failed.\nURL: {grpcUrl}\n\nPlease check your connection settings.", "Save Warning", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // If we can't get the main service instance, show a message
                    MessageBox.Show($"GRPC settings saved successfully.\nURL: {grpcUrl}", "Save Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the buttons
                saveBtn.Enabled = true;
                testConnectionBtn.Enabled = true;
            }
        }
        
        private void ShowSettingsServiceUnavailableMessage(string operation)
        {
            string message = $"Settings service not available. Unable to {operation} settings.";
            MessageBox.Show(message, "Service Unavailable", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            // Log the error for debugging purposes
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        private async void updateConfigBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Pilih File Konfigurasi";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return; // User batal pilih file

                    string path = openFileDialog.FileName;

                    if (!File.Exists(path))
                    {
                        MessageBox.Show(this,
                            $"File tidak ditemukan: {path}",
                            "Update Config",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Ensure GRPC service is initialized and connected before calling UpdateConfigAsync
                    if (_grpcService == null)
                    {
                        // Get the URL from the textbox
                        string grpcUrl = urlHostGrpcTextBox.Text.Trim();
                        
                        // Create new GRPC service with a mock settings service that provides the URL
                        var mockSettings = new MockGrpcSettingsService(grpcUrl);
                        _grpcService = new GrpcService(mockSettings);
                    }
                    
                    // Check if the service is already connected by checking if _client is set
                    // We need to use reflection to access the private _client field
                    var clientField = typeof(GrpcService).GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var client = clientField?.GetValue(_grpcService);
                    
                    // If not connected, try to connect
                    if (client == null)
                    {
                        bool isConnected = await _grpcService.StartAsync();
                        if (!isConnected)
                        {
                            MessageBox.Show(this,
                                "Failed to connect to GRPC server. Please check your connection settings.",
                                "Update Config",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                    
                    // Kirim file ke gRPC
                    // Ensure the gRPC service is connected before calling UpdateConfigAsync
                    if (_grpcService == null)
                    {
                        MessageBox.Show(this,
                            "gRPC service is not initialized.",
                            "Update Config",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    
                    // Try to connect if not already connected
                    if (!_grpcService.IsConnected)
                    {
                        bool isConnected = await _grpcService.StartAsync();
                        if (!isConnected)
                        {
                            MessageBox.Show(this,
                                "Failed to connect to gRPC service.",
                                "Update Config",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                    
                    var resp = await _grpcService.UpdateConfigAsync(path);

                    Console.WriteLine($"Status : {resp.Status}");
                    Console.WriteLine($"Message: {resp.Message}");

                    // Tampilkan hasil
                    MessageBox.Show(this,
                        $"Config update finished.\r\nStatus : {resp.Status}\r\nMessage: {resp.Message}",
                        "Update Config",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Gagal update config:\r\n{ex.Message}",
                    "Update Config",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}