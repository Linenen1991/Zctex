using System;

namespace WpfUsercontrol.ViewModels
{
    public sealed class SearchRuleViewModel : ViewModelBase
    {
        private string _searchPatternToRed;
        private string _searchPatternToDarkRed;
        private string _prefix;
        private string _suffix;
        private string _fixedCandidate;

        public SearchRuleViewModel()
        {
            _fixedCandidate = "User";
            _prefix = string.Empty;
            _suffix = string.Empty;
        }

        public string SearchPatternToRed
        {
            get => _searchPatternToRed;
            set => SetProperty(ref _searchPatternToRed, value);
        }

        public string SearchPatternToDarkRed
        {
            get => _searchPatternToDarkRed;
            set => SetProperty(ref _searchPatternToDarkRed, value);
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.EndsWith("*", StringComparison.Ordinal))
                {
                    return;
                }

                if (SetProperty(ref _prefix, value))
                {
                    OnPropertyChanged(nameof(CombinedPattern));
                }
            }
        }

        public string Suffix
        {
            get => _suffix;
            set
            {
                if (SetProperty(ref _suffix, value))
                {
                    OnPropertyChanged(nameof(CombinedPattern));
                }
            }
        }

        public string FixedCandidate
        {
            get => _fixedCandidate;
            set
            {
                if (SetProperty(ref _fixedCandidate, value))
                {
                    OnPropertyChanged(nameof(CombinedPattern));
                }
            }
        }

        public string CombinedPattern => $"{Prefix}[{FixedCandidate}]{Suffix}";
    }
}
