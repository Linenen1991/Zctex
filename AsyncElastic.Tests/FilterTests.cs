using System.Drawing;
using NUnit.Framework;

namespace AsyncElastic.Tests;

public class FilterTests
{
    [TestCase(null, null, true)]
    [TestCase("", "abc", true)]
    [TestCase("*", "abc", true)]
    [TestCase("123*", "1234", true)]
    [TestCase("123*", "01234", false)]
    [TestCase("*abc*", "xxAbCyy", true)]
    [TestCase("a*b*c", "a__B__c", true)]
    [TestCase("a*b*c", "a__B__d", false)]
    public void WildcardMatcher_IsMatch_Works(string? pattern, string? value, bool expected)
    {
        Assert.That(AsyncElastic.WildcardMatcher.IsMatch(pattern, value), Is.EqualTo(expected));
    }

    [Test]
    public void FilterEngine_FirstMatchWins()
    {
        var item = new AsyncElastic.DataItem
        {
            ServiceName = "svc",
            ClassName = "cls",
            MethodName = "m",
            Message = "hello"
        };

        var first = new AsyncElastic.FilterRule
        {
            ServiceNamePattern = "*",
            ClassNamePattern = "*",
            MethodNamePattern = "*",
            MessagePattern = "*",
            MatchColor = Color.Red
        };

        var second = new AsyncElastic.FilterRule
        {
            ServiceNamePattern = "*",
            ClassNamePattern = "*",
            MethodNamePattern = "*",
            MessagePattern = "*",
            MatchColor = Color.Blue
        };

        var matched = AsyncElastic.FilterEngine.TryGetMatchColor(item, new[] { first, second }, out var color);

        Assert.That(matched, Is.True);
        Assert.That(color.ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    }

    [Test]
    public void FilterEngine_NoMatch_ReturnsFalse()
    {
        var item = new AsyncElastic.DataItem
        {
            ServiceName = "svc",
            ClassName = "cls",
            MethodName = "m",
            Message = "hello"
        };

        var rule = new AsyncElastic.FilterRule
        {
            ServiceNamePattern = "nope*",
            ClassNamePattern = "*",
            MethodNamePattern = "*",
            MessagePattern = "*",
            MatchColor = Color.Red
        };

        var matched = AsyncElastic.FilterEngine.TryGetMatchColor(item, new[] { rule }, out _);
        Assert.That(matched, Is.False);
    }
}
