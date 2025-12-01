using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Inspection
{
    /// <summary>
    /// Manager untuk save/load inspection configuration ke/dari JSON
    /// </summary>
    public class InspectionConfigManager
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InspectionPlatform",
            "Configs");

        private static readonly string ResultsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InspectionPlatform",
            "Results");

        static InspectionConfigManager()
        {
            // Create directories if not exist
            Directory.CreateDirectory(ConfigDirectory);
            Directory.CreateDirectory(ResultsDirectory);
        }

        /// <summary>
        /// Save inspection project to JSON file
        /// </summary>
        public static bool SaveProject(InspectionProject project, string fileName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"{project.ProjectName}_{project.ProjectId}.json";

                string filePath = Path.Combine(ConfigDirectory, fileName);

                project.ModifiedDate = DateTime.Now;

                var json = JsonConvert.SerializeObject(project, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include
                });

                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveProject] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load inspection project from JSON file
        /// </summary>
        public static InspectionProject LoadProject(string fileName)
        {
            try
            {
                string filePath = Path.Combine(ConfigDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                var json = File.ReadAllText(filePath);
                var project = JsonConvert.DeserializeObject<InspectionProject>(json);

                return project;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadProject] Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all saved project files
        /// </summary>
        public static List<string> GetAllProjectFiles()
        {
            try
            {
                var files = Directory.GetFiles(ConfigDirectory, "*.json")
                    .Select(Path.GetFileName)
                    .ToList();
                return files;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Get all projects with metadata
        /// </summary>
        public static List<ProjectMetadata> GetAllProjects()
        {
            var projectList = new List<ProjectMetadata>();

            try
            {
                var files = GetAllProjectFiles();
                foreach (var file in files)
                {
                    try
                    {
                        var project = LoadProject(file);
                        if (project != null)
                        {
                            projectList.Add(new ProjectMetadata
                            {
                                FileName = file,
                                ProjectId = project.ProjectId,
                                ProjectName = project.ProjectName,
                                Description = project.Description,
                                CreatedDate = project.CreatedDate,
                                ModifiedDate = project.ModifiedDate,
                                StepCount = project.Steps?.Count ?? 0
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return projectList;
        }

        /// <summary>
        /// Delete project file
        /// </summary>
        public static bool DeleteProject(string fileName)
        {
            try
            {
                string filePath = Path.Combine(ConfigDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save inspection result to JSON
        /// </summary>
        public static bool SaveResult(InspectionResult result, string fileName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"Result_{result.ProjectName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                string filePath = Path.Combine(ResultsDirectory, fileName);

                var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveResult] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load inspection result from JSON
        /// </summary>
        public static InspectionResult LoadResult(string fileName)
        {
            try
            {
                string filePath = Path.Combine(ResultsDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                var json = File.ReadAllText(filePath);
                var result = JsonConvert.DeserializeObject<InspectionResult>(json);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadResult] Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all result files
        /// </summary>
        public static List<string> GetAllResultFiles()
        {
            try
            {
                var files = Directory.GetFiles(ResultsDirectory, "*.json")
                    .Select(Path.GetFileName)
                    .OrderByDescending(f => f)
                    .ToList();
                return files;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Export project to specific path
        /// </summary>
        public static bool ExportProject(InspectionProject project, string exportPath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(project, Formatting.Indented);
                File.WriteAllText(exportPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Import project from specific path
        /// </summary>
        public static InspectionProject ImportProject(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                    return null;

                var json = File.ReadAllText(importPath);
                var project = JsonConvert.DeserializeObject<InspectionProject>(json);

                // Generate new ID untuk avoid conflict
                project.ProjectId = Guid.NewGuid().ToString();
                project.CreatedDate = DateTime.Now;
                project.ModifiedDate = DateTime.Now;

                return project;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get config directory path
        /// </summary>
        public static string GetConfigDirectory() => ConfigDirectory;

        /// <summary>
        /// Get results directory path
        /// </summary>
        public static string GetResultsDirectory() => ResultsDirectory;

        /// <summary>
        /// Create backup of all configs
        /// </summary>
        public static bool BackupAllConfigs(string backupPath)
        {
            try
            {
                if (Directory.Exists(ConfigDirectory))
                {
                    // Copy all files
                    foreach (var file in Directory.GetFiles(ConfigDirectory, "*.json"))
                    {
                        var destFile = Path.Combine(backupPath, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Metadata untuk list projects
    /// </summary>
    public class ProjectMetadata
    {
        public string FileName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int StepCount { get; set; }
    }
}