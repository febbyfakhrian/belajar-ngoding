using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsFormsApp1.Core.Entities.Models;

namespace WindowsFormsApp1.Infrastructure.Services
{
    public class InspectionLogger
    {
        private List<InspectionResult> logs = new List<InspectionResult>();
        private string folder;

        public InspectionLogger(string folderPath = null)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");
            }
            else
            {
                folder = folderPath;
            }
            Directory.CreateDirectory(folder);
        }

        public void AddLog(InspectionResult result)
        {
            logs.Add(result);
        }

        public void SaveToJson(string fileName = null)
        {
            if (logs.Count == 0) return;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = string.Format("Inspection_{0:yyyyMMdd_HHmmss}.json", DateTime.Now);
            }

            string filePath = Path.Combine(folder, fileName);

            var summary = new CycleTimeSummary
            {
                Timestamp = DateTime.Now,
                Logs = logs,
                AverageMs = logs.Count > 0 ? Math.Round(Average(), 2) : 0,
                MinMs = logs.Count > 0 ? Math.Round(Min(), 2) : 0,
                MaxMs = logs.Count > 0 ? Math.Round(Max(), 2) : 0
            };

            string json = JsonConvert.SerializeObject(summary, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Console.WriteLine("✅ Log tersimpan di: " + filePath);
        }

        private double Average()
        {
            if (logs.Count == 0) return 0;
            return logs.Sum(x => x.CycleTimeMs) / logs.Count;
        }

        private double Min()
        {
            if (logs.Count == 0) return 0;
            return logs.Min(x => x.CycleTimeMs);
        }

        private double Max()
        {
            if (logs.Count == 0) return 0;
            return logs.Max(x => x.CycleTimeMs);
        }

        public List<InspectionResult> GetLogs()
        {
            return logs;
        }
    }
}