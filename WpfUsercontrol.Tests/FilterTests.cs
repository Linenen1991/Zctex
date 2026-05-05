using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WpfUsercontrol.ViewModels;
using System.Windows.Media;

namespace WpfUsercontrol.Tests
{
    public class TransformMessageTests
    {
        [SetUp]
        public void SetUp()
        {
            DummyMasker.Reset();
        }

        [Test]
        public void TransformMessage_SingleRecipeMatch_KeepsPreviewAndCreatesMaskedText()
        {
            var result = MaskingConfigViewModel.TransformMessage(
                @"RecipePath is D:\OTEL\My.xml",
                Rules(new SearchRuleViewModel
                {
                    Prefix = @"OTEL\",
                    FixedCandidate = "DummayRecipe",
                    Suffix = ".xml"
                }));

            Assert.That(result.Text, Is.EqualTo(@"RecipePath is D:\OTEL\MyDummayRecipe_1.xml"));
            Assert.That(result.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_1.xml"));
            AssertHasSegment(result, "My", Brushes.White, Brushes.DarkRed);
            AssertHasSegment(result, "DummayRecipe_1", Brushes.Black, Brushes.LightGreen);
        }

        [Test]
        public void TransformMessage_MultipleRules_MasksRecipeAndId()
        {
            var result = MaskingConfigViewModel.TransformMessage(
                @"RecipePath is D:\OTEL\My.xml, And Id=10, ",
                Rules(
                    new SearchRuleViewModel
                    {
                        Prefix = @"OTEL\",
                        FixedCandidate = "DummayRecipe",
                        Suffix = ".xml"
                    },
                    new SearchRuleViewModel
                    {
                        Prefix = "Id=",
                        FixedCandidate = "DummayID",
                        Suffix = ","
                    }));

            Assert.That(result.Text, Is.EqualTo(@"RecipePath is D:\OTEL\MyDummayRecipe_1.xml, And Id=10DummayID_1, "));
            Assert.That(result.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_1.xml, And Id=DummayID_1, "));
        }

        [Test]
        public void TransformMessage_SameOriginalValue_ReusesSameMask()
        {
            var rules = Rules(new SearchRuleViewModel
            {
                Prefix = @"OTEL\",
                FixedCandidate = "DummayRecipe",
                Suffix = ".xml"
            });

            var first = MaskingConfigViewModel.TransformMessage(@"RecipePath is D:\OTEL\My.xml", rules);
            var second = MaskingConfigViewModel.TransformMessage(@"RecipePath is D:\OTEL\My.xml", rules);

            Assert.That(first.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_1.xml"));
            Assert.That(second.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_1.xml"));
        }

        [Test]
        public void TransformMessage_DifferentOriginalValues_UsesDifferentMasks()
        {
            var rules = Rules(new SearchRuleViewModel
            {
                Prefix = @"OTEL\",
                FixedCandidate = "DummayRecipe",
                Suffix = ".xml"
            });

            var first = MaskingConfigViewModel.TransformMessage(@"RecipePath is D:\OTEL\My.xml", rules);
            var second = MaskingConfigViewModel.TransformMessage(@"RecipePath is D:\OTEL\Your.xml", rules);

            Assert.That(first.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_1.xml"));
            Assert.That(second.MaskedText, Is.EqualTo(@"RecipePath is D:\OTEL\DummayRecipe_2.xml"));
        }

        [Test]
        public void TransformMessage_NoMatch_ReturnsOriginalText()
        {
            const string originalText = "User=alice; Password=secret; ABC123";

            var result = MaskingConfigViewModel.TransformMessage(
                originalText,
                Rules(new SearchRuleViewModel
                {
                    Prefix = @"OTEL\",
                    FixedCandidate = "DummayRecipe",
                    Suffix = ".xml"
                }));

            Assert.That(result.Text, Is.EqualTo(originalText));
            Assert.That(result.MaskedText, Is.EqualTo(originalText));
            Assert.That(result.Segments.Count, Is.EqualTo(1));
            Assert.That(result.Segments[0].Text, Is.EqualTo(originalText));
        }

        [TestCase("", ".xml", "DummayRecipe")]
        [TestCase(@"OTEL\", "", "DummayRecipe")]
        [TestCase(@"OTEL\", ".xml", "")]
        public void TransformMessage_IncompleteRule_ReturnsOriginalText(string prefix, string suffix, string fixedCandidate)
        {
            const string originalText = @"RecipePath is D:\OTEL\My.xml";

            var result = MaskingConfigViewModel.TransformMessage(
                originalText,
                Rules(new SearchRuleViewModel
                {
                    Prefix = prefix,
                    FixedCandidate = fixedCandidate,
                    Suffix = suffix
                }));

            Assert.That(result.Text, Is.EqualTo(originalText));
            Assert.That(result.MaskedText, Is.EqualTo(originalText));
        }

        [Test]
        public void FilterCommand_SearchesOriginalMessage_NotPreviewMessage()
        {
            var viewModel = new MaskingConfigViewModel();

            viewModel.FilterText = "DummayRecipe_1";
            viewModel.FilterCommand.Execute(null);

            Assert.That(viewModel.DataFromServerView.Cast<ServerLogEntryViewModel>().Count(), Is.EqualTo(0));

            viewModel.FilterText = "Id=10";
            viewModel.FilterCommand.Execute(null);

            Assert.That(viewModel.DataFromServerView.Cast<ServerLogEntryViewModel>().Count(), Is.EqualTo(2));
        }

        private static IReadOnlyList<SearchRuleViewModel> Rules(params SearchRuleViewModel[] rules)
        {
            return rules;
        }

        private static void AssertHasSegment(
            MaskingConfigViewModel.TransformResult result,
            string text,
            Brush foreground,
            Brush background)
        {
            Assert.That(
                result.Segments.Any(segment =>
                    segment.Text == text &&
                    ReferenceEquals(segment.Foreground, foreground) &&
                    ReferenceEquals(segment.Background, background)),
                Is.True);
        }
    }
}
