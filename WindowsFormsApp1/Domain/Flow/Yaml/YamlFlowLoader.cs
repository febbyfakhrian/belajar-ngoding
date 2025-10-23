using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;

namespace WindowsFormsApp1.Domain.Flow.Yaml
{
    public static class YamlFlowLoader
    {
        public static YamlFlowDefinition Load(string path)
        {
            var yaml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<YamlFlowDefinition>(yaml);
        }
    }
}
