using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfUsercontrol.ViewModels
{
    public sealed class MaskingConfigViewModel : ViewModelBase
    {
        private string _filterText;

        public MaskingConfigViewModel()
        {
            FixedCandidates = new ReadOnlyCollection<string>(new[] { "Recipe_A", "RecipeID_A" });

            SearchRules = new ObservableCollection<SearchRuleViewModel>
            {
                new SearchRuleViewModel
                {
                    Prefix = "123",
                    FixedCandidate = "Recipe_A",
                    Suffix = "133"
                }
            };

            DataFromServer = new ObservableCollection<ServerLogEntryViewModel>
            {
                new ServerLogEntryViewModel
                {
                    Message = "RecipePath is D:\\OTEL\\My.xml, And Id=10, ",
                    OriginalLogInformation = "RecipePath is D:\\OTEL\\My.xml, And Id=10, ",
                    LogInformation = "RecipePath is D:\\OTEL\\My.xml, And Id=10, "
                },
                new ServerLogEntryViewModel
                {
                    Message = "User=alice; Password=secret; ABC123",
                    OriginalLogInformation = "User=alice; Password=secret; ABC123",
                    LogInformation = "User=alice; Password=secret; ABC123"
                }
            };

            DataFromServerView = CollectionViewSource.GetDefaultView(DataFromServer);
            DataFromServerView.Filter = FilterPredicate;

            StartMatchCommand = new RelayCommand(ApplyMatch);
            FilterCommand = new RelayCommand(() => DataFromServerView.Refresh());
            RemoveRuleCommand = new RelayCommand(parameter =>
            {
                var rule = parameter as SearchRuleViewModel;
                if (rule == null)
                {
                    return;
                }

                SearchRules.Remove(rule);
            });

            ApplyMatch();
        }

        public ReadOnlyCollection<string> FixedCandidates { get; }

        public ObservableCollection<SearchRuleViewModel> SearchRules { get; }

        public ObservableCollection<ServerLogEntryViewModel> DataFromServer { get; }

        public ICollectionView DataFromServerView { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty(ref _filterText, value);
            }
        }

        public ICommand StartMatchCommand { get; }

        public ICommand FilterCommand { get; }

        public ICommand RemoveRuleCommand { get; }

        private bool FilterPredicate(object item)
        {
            var entry = item as ServerLogEntryViewModel;
            if (entry == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FilterText))
            {
                return true;
            }

            return (entry.Message ?? string.Empty).IndexOf(FilterText, StringComparison.Ordinal) >= 0;
        }

        private void ApplyMatch()
        {
            var rulesSnapshot = SearchRules.ToList();
            foreach (var entry in DataFromServer)
            {
                //entry.MessageSegments = new ObservableCollection<TextSegment>(
                //    BuildSegments(entry.Message ?? string.Empty, rulesSnapshot));

                var originalLog = entry.OriginalLogInformation;
                if (string.IsNullOrEmpty(originalLog))
                {
                    originalLog = entry.LogInformation;
                    if (string.IsNullOrEmpty(originalLog))
                    {
                        originalLog = entry.Message ?? string.Empty;
                    }

                    entry.OriginalLogInformation = originalLog;
                }

                var logResult = TransformLogInformation(originalLog, rulesSnapshot);
                entry.LogInformation = logResult.Text;
                entry.LogInformationSegments = new ObservableCollection<TextSegment>(logResult.Segments);
            }
        }

        private static TransformResult TransformLogInformation(string originalText, IReadOnlyList<SearchRuleViewModel> rules)
        {
            if (string.IsNullOrEmpty(originalText))
            {
                return new TransformResult(string.Empty, new[] { new TextSegment(string.Empty, Brushes.Black) });
            }

            var operations = new List<InsertOperation>();
            var occupiedMiddleRanges = new List<TextRange>();
            var usedInsertPositions = new HashSet<int>();

            foreach (var rule in rules)
            {
                var prefix = rule?.Prefix ?? string.Empty;
                var suffix = rule?.Suffix ?? string.Empty;
                var fixedCandidate = rule?.FixedCandidate ?? string.Empty;

                if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix) || string.IsNullOrEmpty(fixedCandidate))
                {
                    continue;
                }

                var searchIndex = 0;
                while (searchIndex < originalText.Length)
                {
                    var prefixPos = originalText.IndexOf(prefix, searchIndex, StringComparison.Ordinal);
                    if (prefixPos < 0)
                    {
                        break;
                    }

                    var middleStart = prefixPos + prefix.Length;
                    var suffixPos = originalText.IndexOf(suffix, middleStart, StringComparison.Ordinal);
                    if (suffixPos < 0)
                    {
                        searchIndex = prefixPos + 1;
                        continue;
                    }

                    if (!usedInsertPositions.Contains(suffixPos) && !Overlaps(middleStart, suffixPos, occupiedMiddleRanges))
                    {
                        operations.Add(new InsertOperation(prefixPos, middleStart, suffixPos, fixedCandidate));
                        usedInsertPositions.Add(suffixPos);
                        if (suffixPos > middleStart)
                        {
                            occupiedMiddleRanges.Add(new TextRange(middleStart, suffixPos));
                        }
                    }

                    searchIndex = suffixPos + suffix.Length;
                }
            }

            if (operations.Count == 0)
            {
                return new TransformResult(originalText, new[] { new TextSegment(originalText, Brushes.Black) });
            }

            var textBuilder = new StringBuilder(originalText);
            operations.Sort(InsertOperation.InsertAtDescending);
            foreach (var operation in operations)
            {
                textBuilder.Insert(operation.InsertAt, operation.InsertText);
            }

            var finalText = textBuilder.ToString();

            operations.Sort(InsertOperation.InsertAtAscending);
            var insertPoints = new List<InsertPoint>(operations.Count);
            foreach (var operation in operations)
            {
                insertPoints.Add(new InsertPoint(operation.InsertAt, operation.InsertText.Length));
            }

            var spans = new List<ColoredSpan>(operations.Count * 2);
            foreach (var operation in operations)
            {
                var middleStartOffset = OffsetAt(operation.MiddleStart, insertPoints);
                var suffixOffset = OffsetAt(operation.InsertAt, insertPoints);

                var middleStartFinal = operation.MiddleStart + middleStartOffset;
                var middleEndFinal = operation.InsertAt + suffixOffset;
                if (middleEndFinal > middleStartFinal)
                {
                    spans.Add(new ColoredSpan(middleStartFinal, middleEndFinal, Brushes.White, Brushes.DarkRed));
                }

                var insertedStartFinal = operation.InsertAt + suffixOffset;
                spans.Add(new ColoredSpan(insertedStartFinal, insertedStartFinal + operation.InsertText.Length, Brushes.Black, Brushes.LightGreen));
            }

            spans.Sort(ColoredSpan.ByStartAscending);

            var segments = new List<TextSegment>();
            var cursor = 0;
            foreach (var span in spans)
            {
                if (span.Start > cursor)
                {
                    segments.Add(new TextSegment(finalText.Substring(cursor, span.Start - cursor), Brushes.Black));
                }

                if (span.End > span.Start)
                {
                    segments.Add(new TextSegment(finalText.Substring(span.Start, span.End - span.Start), span.Foreground, span.Background));
                }

                cursor = span.End;
            }

            if (cursor < finalText.Length)
            {
                segments.Add(new TextSegment(finalText.Substring(cursor), Brushes.Black));
            }

            return new TransformResult(finalText, segments);
        }

        private static bool Overlaps(int start, int end, List<TextRange> occupiedRanges)
        {
            foreach (var range in occupiedRanges)
            {
                if (start < range.End && range.Start < end)
                {
                    return true;
                }
            }

            return false;
        }

        private static int OffsetAt(int originalIndex, List<InsertPoint> insertPoints)
        {
            var offset = 0;
            foreach (var point in insertPoints)
            {
                if (point.Position >= originalIndex)
                {
                    break;
                }

                offset += point.Length;
            }

            return offset;
        }

        private readonly struct TransformResult
        {
            public TransformResult(string text, IReadOnlyList<TextSegment> segments)
            {
                Text = text;
                Segments = segments;
            }

            public string Text { get; }
            public IReadOnlyList<TextSegment> Segments { get; }
        }

        private readonly struct InsertPoint
        {
            public InsertPoint(int position, int length)
            {
                Position = position;
                Length = length;
            }

            public int Position { get; }
            public int Length { get; }
        }

        private readonly struct TextRange
        {
            public TextRange(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; }
            public int End { get; }
        }

        private readonly struct InsertOperation
        {
            public InsertOperation(int prefixPos, int middleStart, int insertAt, string insertText)
            {
                PrefixPos = prefixPos;
                MiddleStart = middleStart;
                InsertAt = insertAt;
                InsertText = insertText ?? string.Empty;
            }

            public int PrefixPos { get; }
            public int MiddleStart { get; }
            public int InsertAt { get; }
            public string InsertText { get; }

            public static int InsertAtAscending(InsertOperation x, InsertOperation y)
            {
                return x.InsertAt.CompareTo(y.InsertAt);
            }

            public static int InsertAtDescending(InsertOperation x, InsertOperation y)
            {
                return y.InsertAt.CompareTo(x.InsertAt);
            }
        }

        private readonly struct ColoredSpan
        {
            public ColoredSpan(int start, int end, Brush foreground, Brush background)
            {
                Start = start;
                End = end;
                Foreground = foreground;
                Background = background;
            }

            public int Start { get; }
            public int End { get; }
            public Brush Foreground { get; }
            public Brush Background { get; }

            public static int ByStartAscending(ColoredSpan x, ColoredSpan y)
            {
                return x.Start.CompareTo(y.Start);
            }
        }

        private static IReadOnlyList<TextSegment> BuildSegments(string text, IReadOnlyList<SearchRuleViewModel> rules)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new[] { new TextSegment(string.Empty, Brushes.Black) };
            }

            var matchSpecs = new List<MatchSpec>();
            foreach (var rule in rules)
            {
                if (!string.IsNullOrEmpty(rule?.SearchPatternToRed))
                {
                    matchSpecs.Add(new MatchSpec(rule.SearchPatternToRed, Brushes.Red, priority: 2));
                }

                if (!string.IsNullOrEmpty(rule?.SearchPatternToDarkRed))
                {
                    matchSpecs.Add(new MatchSpec(rule.SearchPatternToDarkRed, Brushes.DarkRed, priority: 1));
                }
            }

            if (matchSpecs.Count == 0)
            {
                return new[] { new TextSegment(text, Brushes.Black) };
            }

            var allMatches = new List<TextMatch>();
            foreach (var spec in matchSpecs)
            {
                var index = 0;
                while (index < text.Length)
                {
                    var found = text.IndexOf(spec.Pattern, index, StringComparison.Ordinal);
                    if (found < 0)
                    {
                        break;
                    }

                    allMatches.Add(new TextMatch(found, spec.Pattern.Length, spec.Background, spec.Priority));
                    index = found + 1;
                }
            }

            if (allMatches.Count == 0)
            {
                return new[] { new TextSegment(text, Brushes.Black) };
            }

            allMatches.Sort(TextMatchComparer.Instance);

            var segments = new List<TextSegment>();
            var cursor = 0;
            var matchIndex = 0;

            while (cursor < text.Length)
            {
                while (matchIndex < allMatches.Count && allMatches[matchIndex].Start < cursor)
                {
                    matchIndex++;
                }

                if (matchIndex >= allMatches.Count)
                {
                    segments.Add(new TextSegment(text.Substring(cursor), Brushes.Black));
                    break;
                }

                var nextStart = allMatches[matchIndex].Start;
                if (nextStart > cursor)
                {
                    segments.Add(new TextSegment(text.Substring(cursor, nextStart - cursor), Brushes.Black));
                    cursor = nextStart;
                    continue;
                }

                var best = allMatches[matchIndex];
                var scan = matchIndex + 1;
                while (scan < allMatches.Count && allMatches[scan].Start == cursor)
                {
                    var candidate = allMatches[scan];
                    if (candidate.Priority > best.Priority ||
                        (candidate.Priority == best.Priority && candidate.Length > best.Length))
                    {
                        best = candidate;
                    }
                    scan++;
                }

                var boundedLength = best.Length;
                if (cursor + boundedLength > text.Length)
                {
                    boundedLength = text.Length - cursor;
                }

                segments.Add(new TextSegment(text.Substring(cursor, boundedLength), Brushes.White, best.Background));
                cursor += boundedLength;
                matchIndex = scan;
            }

            return segments;
        }

        private readonly struct MatchSpec
        {
            public MatchSpec(string pattern, Brush background, int priority)
            {
                Pattern = pattern;
                Background = background;
                Priority = priority;
            }

            public string Pattern { get; }
            public Brush Background { get; }
            public int Priority { get; }
        }

        private readonly struct TextMatch
        {
            public TextMatch(int start, int length, Brush background, int priority)
            {
                Start = start;
                Length = length;
                Background = background;
                Priority = priority;
            }

            public int Start { get; }
            public int Length { get; }
            public Brush Background { get; }
            public int Priority { get; }
        }

        private sealed class TextMatchComparer : IComparer<TextMatch>
        {
            public static TextMatchComparer Instance { get; } = new TextMatchComparer();

            public int Compare(TextMatch x, TextMatch y)
            {
                var byStart = x.Start.CompareTo(y.Start);
                if (byStart != 0)
                {
                    return byStart;
                }

                var byPriority = y.Priority.CompareTo(x.Priority);
                if (byPriority != 0)
                {
                    return byPriority;
                }

                return y.Length.CompareTo(x.Length);
            }
        }
    }
}
