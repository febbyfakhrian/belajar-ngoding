using System;
using System.Windows.Forms;
using WindowsFormsApp1.Infrastructure.Hardware.Grpc;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WindowsFormsApp1.Core.Interfaces;
using System.Diagnostics; // Added for logging

namespace WindowsFormsApp1
{
    public partial class GRPCDialog : Form
    {
        private GrpcService _grpcService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService; // Made readonly

        // Constructor with dependency injection for both service provider and settings service
        public GRPCDialog(IServiceProvider serviceProvider, ISettingsService settingsService = null)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService ?? GetSettingsServiceFromProvider();
            InitializeComponent();
            
            // Load saved settings if they exist
            LoadSettings();
        }

        // Legacy constructor for backward compatibility
        public GRPCDialog() : this(null, null)
        {
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
                
                // Dispose of existing service if any
                if (_grpcService != null)
                {
                    _grpcService.Dispose();
                    _grpcService = null;
                }
                
                // Create new GRPC service with a mock settings service that provides the URL
                var mockSettings = new MockGrpcSettingsService(grpcUrl);
                _grpcService = new GrpcService(mockSettings);
                
                // Test the connection
                bool isConnected = await _grpcService.StartAsync();
                
                if (isConnected)
                {
                    saveBtn.Enabled = true;
                    MessageBox.Show($"Successfully connected to GRPC server at {grpcUrl}", "Connection Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    saveBtn.Enabled = false;
                    MessageBox.Show($"Failed to connect to GRPC server at {grpcUrl}", "Connection Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                // Dispose of the service after testing
                if (_grpcService != null)
                {
                    _grpcService.Dispose();
                    _grpcService = null;
                }
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
                
                // Dispose of existing service if any
                if (_grpcService != null)
                {
                    _grpcService.Dispose();
                    _grpcService = null;
                }
                
                // Create new GRPC service with a mock settings service that provides the URL
                var mockSettings = new MockGrpcSettingsService(grpcUrl);
                _grpcService = new GrpcService(mockSettings);
                
                // Test the connection
                bool isConnected = await _grpcService.StartAsync();
                
                if (isConnected)
                {
                    // Connection successful, save settings to database
                    try
                    {
                        // Save the GRPC URL
                        _settingsService.SetSetting("grpc", "server_url", grpcUrl);
                        
                        MessageBox.Show($"GRPC settings saved successfully.\nURL: {grpcUrl}", "Save Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Connection failed, but still save the URL and mark as disconnected
                    try
                    {
                        // Save the GRPC URL
                        _settingsService.SetSetting("grpc", "server_url", grpcUrl);
                        
                        MessageBox.Show($"GRPC URL saved but connection failed.\nURL: {grpcUrl}\n\nPlease check your connection settings.", "Save Warning", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", "Connection Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the buttons
                saveBtn.Enabled = true;
                testConnectionBtn.Enabled = true;
                
                // Dispose of the service after operation
                if (_grpcService != null)
                {
                    _grpcService.Dispose();
                    _grpcService = null;
                }
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
    }
}