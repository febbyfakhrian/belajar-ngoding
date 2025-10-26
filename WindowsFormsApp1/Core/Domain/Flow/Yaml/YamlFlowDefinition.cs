using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Domain.Flow.Yaml
{
    public sealed class YamlFlowDefinition
    {
        public List<string> States { get; set; }
        public List<string> Triggers { get; set; }
        public List<YamlTransition> Transitions { get; set; }
        public List<YamlAutoTrigger> AutoTriggers { get; set; }

        public YamlFlowDefinition()
        {
            States = new List<string>();
            Triggers = new List<string>();
            Transitions = new List<YamlTransition>();
            AutoTriggers = new List<YamlAutoTrigger>();
        }
    }

    public sealed class YamlTransition
    {
        public string From { get; set; }
        public string Trigger { get; set; }
        public string To { get; set; }
        public List<string> OnEnter { get; set; }
        public List<string> OnExit { get; set; }

        public YamlTransition()
        {
            OnEnter = new List<string>();
            OnExit = new List<string>();
        }
    }

    public sealed class YamlAutoTrigger
    {
        public string When { get; set; }
        public string Emit { get; set; }
        public List<string> AllowedFrom { get; set; }

        public YamlAutoTrigger()
        {
            AllowedFrom = new List<string>();
        }
    }
}
