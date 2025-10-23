using System;
using System.Linq;

namespace WindowsFormsApp1.Domain.Flow.Yaml
{
    public static class FlowValidator
    {
        public static void Validate(YamlFlowDefinition def)
        {
            if (def.States.Count == 0) throw new InvalidOperationException("No states in YAML.");
            var stateSet = def.States.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var trigSet = def.Triggers.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var t in def.Transitions)
            {
                if (!stateSet.Contains(t.From)) throw new InvalidOperationException($"Unknown state: {t.From}");
                if (!stateSet.Contains(t.To)) throw new InvalidOperationException($"Unknown state: {t.To}");
                if (!trigSet.Contains(t.Trigger)) throw new InvalidOperationException($"Unknown trigger: {t.Trigger}");
            }
        }
    }
}
