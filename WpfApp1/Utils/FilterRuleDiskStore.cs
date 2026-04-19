using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using WpfApp1.Models;

namespace WpfApp1.Utils;

internal static class FilterRuleDiskStore
{
    public const string FileName = "filterList.xml";

    public static string GetDefaultPath() =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

    public static void Save(string path, IEnumerable<FilterRule> rules)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        if (rules is null) throw new ArgumentNullException(nameof(rules));

        var doc = new XDocument(
            new XElement("FilterRules",
                rules.Select(ToElement)
            )
        );

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        doc.Save(path);
    }

    public static List<FilterRule> Load(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));

        if (!File.Exists(path))
        {
            return new List<FilterRule>();
        }

        var doc = XDocument.Load(path);
        var root = doc.Root;
        if (root is null || root.Name != "FilterRules")
        {
            return new List<FilterRule>();
        }

        return root.Elements("Rule").Select(FromElement).ToList();
    }

    private static XElement ToElement(FilterRule rule) =>
        new XElement("Rule",
            new XAttribute(nameof(FilterRule.ServiceNamePattern), rule.ServiceNamePattern ?? ""),
            new XAttribute(nameof(FilterRule.ClassNamePattern), rule.ClassNamePattern ?? ""),
            new XAttribute(nameof(FilterRule.MethodNamePattern), rule.MethodNamePattern ?? ""),
            new XAttribute(nameof(FilterRule.MessagePattern), rule.MessagePattern ?? ""),
            new XAttribute(nameof(FilterRule.MatchColor), ToArgbHex(rule.MatchColor))
        );

    private static FilterRule FromElement(XElement element)
    {
        var service = (string?)element.Attribute(nameof(FilterRule.ServiceNamePattern)) ?? "";
        var @class = (string?)element.Attribute(nameof(FilterRule.ClassNamePattern)) ?? "";
        var method = (string?)element.Attribute(nameof(FilterRule.MethodNamePattern)) ?? "";
        var message = (string?)element.Attribute(nameof(FilterRule.MessagePattern)) ?? "";
        var colorText = (string?)element.Attribute(nameof(FilterRule.MatchColor)) ?? "#FFFFD700"; // LightGoldenrodYellow-ish

        return new FilterRule
        {
            ServiceNamePattern = service,
            ClassNamePattern = @class,
            MethodNamePattern = method,
            MessagePattern = message,
            MatchColor = ParseColor(colorText)
        };
    }

    private static string ToArgbHex(Color color) =>
        $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static Color ParseColor(string text)
    {
        text = (text ?? "").Trim();
        if (!text.StartsWith("#", StringComparison.Ordinal))
        {
            return Colors.LightGoldenrodYellow;
        }

        var hex = text[1..];
        if (hex.Length == 6)
        {
            // RRGGBB
            return Color.FromRgb(
                ParseByte(hex[0..2]),
                ParseByte(hex[2..4]),
                ParseByte(hex[4..6])
            );
        }

        if (hex.Length == 8)
        {
            // AARRGGBB
            return Color.FromArgb(
                ParseByte(hex[0..2]),
                ParseByte(hex[2..4]),
                ParseByte(hex[4..6]),
                ParseByte(hex[6..8])
            );
        }

        return Colors.LightGoldenrodYellow;
    }

    private static byte ParseByte(string hex) =>
        byte.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
}

