using System;
using YamlDotNet.Serialization;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace WindowsFormsApp1.Core.Domain.Flow.Dag
{
    class DagFlowLoader
    {
        public static DagDefinition Load(string path)
        {
            var yaml = File.ReadAllText(path);
            var des = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            return des.Deserialize<DagDefinition>(yaml);
        }

        public static DagDefinition LoadJson(string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            Debug.WriteLine(path);
            if (!File.Exists(path))
                throw new FileNotFoundException("DAG JSON not found", path);

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<DagDefinition>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
