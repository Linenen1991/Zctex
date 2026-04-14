using System.Drawing;

namespace AsyncElastic;

public static class FilterEngine
{
    public static bool TryGetMatchColor(DataItem item, IEnumerable<FilterRule> rules, out Color color)
    {
        foreach (var rule in rules)
        {
            if (!WildcardMatcher.IsMatch(rule.ServiceNamePattern, item.ServiceName))
            {
                continue;
            }

            if (!WildcardMatcher.IsMatch(rule.ClassNamePattern, item.ClassName))
            {
                continue;
            }

            if (!WildcardMatcher.IsMatch(rule.MethodNamePattern, item.MethodName))
            {
                continue;
            }

            if (!WildcardMatcher.IsMatch(rule.MessagePattern, item.Message))
            {
                continue;
            }

            color = rule.MatchColor;
            return true;
        }

        color = default;
        return false;
    }
}

