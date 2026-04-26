using System;

namespace WpfUsercontrol.ViewModels
{
    public sealed class SearchRuleViewModel : ViewModelBase
    {
        private string _searchPatternToRed;
        private string _searchPatternToDarkRed;

        private string _serviceName;
        private string _className;
        private string _methodName;


        private string _prefix;
        private string _suffix;
        private string _fixedCandidate;

        public SearchRuleViewModel()
        {
            _fixedCandidate = "DummayRecipe";
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

        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
        }

        public string MethodName
        {
            get => _methodName;
            set => SetProperty(ref _methodName, value);
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.EndsWith("*", StringComparison.Ordinal))
                {
                    OnPropertyChanged(nameof(Prefix));
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
                if (!string.IsNullOrEmpty(value) && value.StartsWith("*", StringComparison.Ordinal))
                {
                    OnPropertyChanged(nameof(Suffix));
                    return;
                }

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

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(SearchPatternToRed) &&
            string.IsNullOrWhiteSpace(SearchPatternToDarkRed) &&
            string.IsNullOrWhiteSpace(Prefix) &&
            string.IsNullOrWhiteSpace(Suffix);
    }
}
