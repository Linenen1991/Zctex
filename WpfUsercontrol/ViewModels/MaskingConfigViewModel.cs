using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            FixedCandidates = new ReadOnlyCollection<string>(new[] { "User", "Password" });

            SearchRules = new ObservableCollection<SearchRuleViewModel>
            {
                new SearchRuleViewModel
                {
                    SearchPatternToRed = "123",
                    SearchPatternToDarkRed = "ABC",
                    Prefix = string.Empty,
                    FixedCandidate = "User",
                    Suffix = string.Empty
                }
            };

            DataFromServer = new ObservableCollection<ServerLogEntryViewModel>
            {
                new ServerLogEntryViewModel
                {
                    UserComment = "Demo",
                    Message = "AB123ABC13312111567123ABC9ACA023"
                },
                new ServerLogEntryViewModel
                {
                    UserComment = "Second",
                    Message = "User=alice; Password=secret; ABC123"
                }
            };

            DataFromServerView = CollectionViewSource.GetDefaultView(DataFromServer);
            DataFromServerView.Filter = FilterPredicate;

            StartMatchCommand = new RelayCommand(ApplyMatch);
            FilterCommand = new RelayCommand(() => DataFromServerView.Refresh());

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
                entry.MessageSegments = new ObservableCollection<TextSegment>(
                    BuildSegments(entry.Message ?? string.Empty, rulesSnapshot));
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
