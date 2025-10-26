namespace WindowsFormsApp1.Core.Interfaces
{
    /// <summary>
    /// Interface for application settings service
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value by group and key
        /// </summary>
        /// <typeparam name="T">The type of the setting value</typeparam>
        /// <param name="groupName">The settings group name</param>
        /// <param name="key">The setting key</param>
        /// <returns>The setting value</returns>
        T GetSetting<T>(string groupName, string key);
        
        /// <summary>
        /// Sets a setting value by group and key
        /// </summary>
        /// <param name="groupName">The settings group name</param>
        /// <param name="key">The setting key</param>
        /// <param name="value">The setting value</param>
        void SetSetting(string groupName, string key, object value);
    }
}