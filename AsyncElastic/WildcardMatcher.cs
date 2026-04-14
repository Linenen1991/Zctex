using System.Text;
using System.Text.RegularExpressions;

namespace AsyncElastic;

public static class WildcardMatcher
{
    public static bool IsMatch(string? pattern, string? value)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*")
        {
            return true;
        }

        value ??= "";

        // Glob-style wildcard: '*' matches any sequence. Match is anchored to entire string.
        var regexPattern = BuildRegexPattern(pattern);
        return Regex.IsMatch(
            input: value,
            pattern: regexPattern,
            options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline
        );
    }

    private static string BuildRegexPattern(string pattern)
    {
        var builder = new StringBuilder(capacity: pattern.Length * 2);
        builder.Append('^');

        foreach (var ch in pattern)
        {
            if (ch == '*')
            {
                builder.Append(".*");
                continue;
            }

            builder.Append(Regex.Escape(ch.ToString()));
        }

        builder.Append('$');
        return builder.ToString();
    }
}

