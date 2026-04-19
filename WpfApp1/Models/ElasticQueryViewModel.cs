using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using WpfApp1.Utils;

namespace WpfApp1.Models
{
    public sealed class ElasticQueryViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SynchronizationContext _uiContext;

        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private long _fetchedCount;
        private int _selectedPageIndex;
        private string _statusText = "Idle";
        private string? _lastError;

        private readonly List<IReadOnlyList<LogEntry>> _pages = new List<IReadOnlyList<LogEntry>>();

        private IList<LogEntry>? _targetEntries;
        private Func<IReadOnlyList<FilterRule>>? _getFilterRules;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string EndpointUrl { get; set; } = "http://localhost:9200";

        public string IndexPattern { get; set; } = "logs-*";

        public string TimestampField { get; set; } = "@timestamp";

        public TimeSpan Lookback { get; set; } = TimeSpan.FromHours(24);

        public string EndUtc { get; set; } = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        public int PageSize { get; set; } = 10000;

        public int MaxPages { get; set; } = 7;

        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (SetField(ref _isRunning, value))
                {
                    OnPropertyChanged(nameof(ActionButtonText));
                }
            }
        }

        public string ActionButtonText
        {
            get { return IsRunning ? "Cancel" : "Query"; }
        }

        public long FetchedCount
        {
            get { return _fetchedCount; }
            private set
            {
                if (SetField(ref _fetchedCount, value))
                {
                    OnPropertyChanged(nameof(PageText));
                }
            }
        }

        public int LoadedPagesCount
        {
            get { return _pages.Count; }
        }

        public int SelectedPageIndex
        {
            get { return _selectedPageIndex; }
            set
            {
                if (!SetField(ref _selectedPageIndex, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(CanPrev));
                OnPropertyChanged(nameof(CanNext));
                OnPropertyChanged(nameof(PageText));
                DisplaySelectedPage();
            }
        }

        public bool CanPrev
        {
            get { return SelectedPageIndex > 0; }
        }

        public bool CanNext
        {
            get { return SelectedPageIndex + 1 < LoadedPagesCount; }
        }

        public string PageText
        {
            get
            {
                if (LoadedPagesCount == 0)
                {
                    return "Page: 0/0";
                }

                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Page: {0}/{1}  Fetched: {2}",
                    SelectedPageIndex + 1,
                    LoadedPagesCount,
                    FetchedCount);
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            private set { SetField(ref _statusText, value); }
        }

        public string? LastError
        {
            get { return _lastError; }
            private set { SetField(ref _lastError, value); }
        }

        public Func<JsonElement, LogEntry> ConvertSourceToLogEntry { get; set; }

        public ElasticQueryViewModel()
            : this(SynchronizationContext.Current ?? new SynchronizationContext())
        {
        }

        public ElasticQueryViewModel(SynchronizationContext uiContext)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            _uiContext = uiContext;

            ConvertSourceToLogEntry = source => new LogEntry
            {
                Message = source.GetRawText()
            };
        }

        public void BindTargets(IList<LogEntry> targetEntries, Func<IReadOnlyList<FilterRule>> getFilterRules)
        {
            if (targetEntries == null) throw new ArgumentNullException(nameof(targetEntries));
            if (getFilterRules == null) throw new ArgumentNullException(nameof(getFilterRules));

            _targetEntries = targetEntries;
            _getFilterRules = getFilterRules;
        }

        public async Task StartOrCancelAsync()
        {
            if (IsRunning)
            {
                Cancel();
                return;
            }

            await StartAsync(EndUtc).ConfigureAwait(false);
        }

        public void Cancel()
        {
            var cts = _cts;
            if (cts == null)
            {
                return;
            }

            try
            {
                PostToUi(() => StatusText = "Cancelling...");
            }
            finally
            {
                cts.Cancel();
            }
        }

        public void PrevPage()
        {
            if (!CanPrev)
            {
                return;
            }

            SelectedPageIndex--;
        }

        public void NextPage()
        {
            if (!CanNext)
            {
                return;
            }

            SelectedPageIndex++;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async Task StartAsync(string endUtc)
        {
            if (_targetEntries == null || _getFilterRules == null)
            {
                SetError("Query targets not bound (LogEntries / FilterRules).");
                return;
            }

            DateTimeOffset endTimeUtc;
            string parseError;
            if (!TryParseUtc(endUtc, out endTimeUtc, out parseError))
            {
                SetError(parseError);
                return;
            }

            var startTimeUtc = endTimeUtc - Lookback;

            ResetForNewRun();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            PostToUi(() =>
            {
                IsRunning = true;
                StatusText = string.Format(CultureInfo.InvariantCulture, "Running: 0 docs (0/{0} pages)", MaxPages);
            });

            string? pitId = null;
            try
            {
                var client = CreateClient();
                pitId = await OpenPitAsync(client, IndexPattern, token).ConfigureAwait(false);

                JsonElement[]? searchAfter = null;

                for (var pageIndex = 0; pageIndex < MaxPages; pageIndex++)
                {
                    token.ThrowIfCancellationRequested();

                    using (var responseJson = await SearchPageAsync(
                               client,
                               pitId,
                               startTimeUtc,
                               endTimeUtc,
                               searchAfter,
                               token).ConfigureAwait(false))
                    {
                        var hits = responseJson.RootElement
                            .GetProperty("hits")
                            .GetProperty("hits");

                        if (hits.ValueKind != JsonValueKind.Array || hits.GetArrayLength() == 0)
                        {
                            break;
                        }

                        var rulesSnapshot = _getFilterRules().ToArray();

                        var pageItems = new List<LogEntry>(hits.GetArrayLength());
                        JsonElement[]? lastSort = null;

                        foreach (var hit in hits.EnumerateArray())
                        {
                            token.ThrowIfCancellationRequested();

                            JsonElement source;
                            if (!hit.TryGetProperty("_source", out source) || source.ValueKind == JsonValueKind.Undefined)
                            {
                                continue;
                            }

                            var entry = ConvertSourceToLogEntry(source);
                            ApplyHighlight(entry, rulesSnapshot);
                            pageItems.Add(entry);

                            JsonElement sortEl;
                            if (hit.TryGetProperty("sort", out sortEl) && sortEl.ValueKind == JsonValueKind.Array)
                            {
                                lastSort = sortEl.EnumerateArray().ToArray();
                            }
                        }

                        if (pageItems.Count == 0)
                        {
                            break;
                        }

                        searchAfter = lastSort;

                        PostToUi(() =>
                        {
                            _pages.Add(pageItems);
                            OnPropertyChanged(nameof(LoadedPagesCount));
                            OnPropertyChanged(nameof(CanPrev));
                            OnPropertyChanged(nameof(CanNext));

                            FetchedCount += pageItems.Count;
                            OnPropertyChanged(nameof(PageText));

                            if (_pages.Count == 1)
                            {
                                SelectedPageIndex = 0;
                            }

                            StatusText = string.Format(
                                CultureInfo.InvariantCulture,
                                "Running: {0} docs ({1}/{2} pages)",
                                FetchedCount,
                                LoadedPagesCount,
                                MaxPages);

                            if (SelectedPageIndex == LoadedPagesCount - 1)
                            {
                                DisplaySelectedPage();
                            }
                        });
                    }
                }

                PostToUi(() =>
                {
                    StatusText = token.IsCancellationRequested
                        ? string.Format(CultureInfo.InvariantCulture, "Cancelled: {0} docs ({1}/{2} pages)", FetchedCount, LoadedPagesCount, MaxPages)
                        : string.Format(CultureInfo.InvariantCulture, "Finished: {0} docs ({1}/{2} pages)", FetchedCount, LoadedPagesCount, MaxPages);
                });
            }
            catch (OperationCanceledException)
            {
                PostToUi(() =>
                {
                    StatusText = string.Format(CultureInfo.InvariantCulture, "Cancelled: {0} docs ({1}/{2} pages)", FetchedCount, LoadedPagesCount, MaxPages);
                });
            }
            catch (Exception ex)
            {
                PostToUi(() =>
                {
                    LastError = ex.ToString();
                    StatusText = "Error: " + ex.Message;
                });
            }
            finally
            {
                PostToUi(() => IsRunning = false);

                if (!string.IsNullOrWhiteSpace(pitId))
                {
                    try
                    {
                        var client = CreateClient();
                        await ClosePitAsync(client, pitId, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore cleanup failures
                    }
                }

                _cts?.Dispose();
                _cts = null;
            }
        }

        private void ResetForNewRun()
        {
            PostToUi(() =>
            {
                LastError = null;
                FetchedCount = 0;
                _pages.Clear();
                SelectedPageIndex = 0;
                StatusText = "Starting...";
                ReplaceTarget(Array.Empty<LogEntry>());
                OnPropertyChanged(nameof(LoadedPagesCount));
                OnPropertyChanged(nameof(CanPrev));
                OnPropertyChanged(nameof(CanNext));
                OnPropertyChanged(nameof(PageText));
            });
        }

        private void DisplaySelectedPage()
        {
            if (_targetEntries == null)
            {
                return;
            }

            if (_pages.Count == 0)
            {
                ReplaceTarget(Array.Empty<LogEntry>());
                return;
            }

            var index = Clamp(SelectedPageIndex, 0, _pages.Count - 1);
            ReplaceTarget(_pages[index]);
        }

        private void ReplaceTarget(IReadOnlyList<LogEntry> items)
        {
            if (_targetEntries == null)
            {
                return;
            }

            var range = _targetEntries as ObservableRangeCollection<LogEntry>;
            if (range != null)
            {
                range.ReplaceRange(items);
                return;
            }

            _targetEntries.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                _targetEntries.Add(items[i]);
            }
        }

        private void ApplyHighlight(LogEntry entry, IReadOnlyList<FilterRule> rules)
        {
            entry.ClearHighlight();

            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (string.Equals(rule.ClassNamePattern, entry.ClassName, StringComparison.Ordinal))
                {
                    entry.ApplyHighlight(rule.MatchColor);
                    return;
                }
            }
        }

        private ElasticsearchClient CreateClient()
        {
            var settings = new ElasticsearchClientSettings(new Uri(EndpointUrl));
            return new ElasticsearchClient(settings);
        }

        private static bool TryParseUtc(string input, out DateTimeOffset utc, out string error)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                utc = default(DateTimeOffset);
                error = "EndUtc is empty.";
                return false;
            }

            if (!DateTimeOffset.TryParse(
                    input,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out utc))
            {
                error = "Invalid EndUtc. Use ISO-8601 UTC string like 2026-04-17T00:00:00Z.";
                return false;
            }

            utc = utc.ToUniversalTime();
            error = "";
            return true;
        }

        private static async Task<string> OpenPitAsync(ElasticsearchClient client, string indexPattern, CancellationToken ct)
        {
            var path = indexPattern + "/_pit?keep_alive=5m";
            var response = await client.Transport.RequestAsync<StringResponse>(
                method: HttpMethod.POST,
                path: path,
                data: PostData.Empty,
                requestParameters: null!,
                cancellationToken: ct).ConfigureAwait(false);

            if (response.ApiCallDetails == null || !response.ApiCallDetails.HasSuccessfulStatusCode)
            {
                var code = response.ApiCallDetails != null ? response.ApiCallDetails.HttpStatusCode : null;
                throw new InvalidOperationException("Open PIT failed: " + code);
            }

            using (var doc = JsonDocument.Parse(response.Body ?? ""))
            {
                var pit = doc.RootElement.GetProperty("id").GetString();
                if (string.IsNullOrWhiteSpace(pit))
                {
                    throw new InvalidOperationException("PIT id missing.");
                }
                return pit;
            }
        }

        private Task<JsonDocument> SearchPageAsync(
            ElasticsearchClient client,
            string pitId,
            DateTimeOffset startTimeUtc,
            DateTimeOffset endTimeUtc,
            JsonElement[]? searchAfter,
            CancellationToken ct)
        {
            var sort = new JsonArray
            {
                new JsonObject
                {
                    [TimestampField] = new JsonObject
                    {
                        ["order"] = "desc"
                    }
                },
                new JsonObject
                {
                    ["_shard_doc"] = "desc"
                }
            };

            var range = new JsonObject
            {
                ["gte"] = startTimeUtc.ToString("o", CultureInfo.InvariantCulture),
                ["lte"] = endTimeUtc.ToString("o", CultureInfo.InvariantCulture)
            };

            var body = new JsonObject
            {
                ["size"] = PageSize,
                ["track_total_hits"] = false,
                ["pit"] = new JsonObject
                {
                    ["id"] = pitId,
                    ["keep_alive"] = "5m"
                },
                ["sort"] = sort,
                ["query"] = new JsonObject
                {
                    ["bool"] = new JsonObject
                    {
                        ["filter"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["range"] = new JsonObject
                                {
                                    [TimestampField] = range
                                }
                            }
                        }
                    }
                }
            };

            if (searchAfter != null && searchAfter.Length > 0)
            {
                var arr = new JsonArray();
                for (var i = 0; i < searchAfter.Length; i++)
                {
                    arr.Add(JsonNode.Parse(searchAfter[i].GetRawText()));
                }
                body["search_after"] = arr;
            }

            var json = body.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            return SearchPageRawAsync(client, json, ct);
        }

        private async Task<JsonDocument> SearchPageRawAsync(ElasticsearchClient client, string jsonBody, CancellationToken ct)
        {
            var response = await client.Transport.RequestAsync<StringResponse>(
                method: HttpMethod.POST,
                path: "/_search",
                data: PostData.String(jsonBody),
                requestParameters: null!,
                cancellationToken: ct).ConfigureAwait(false);

            if (response.ApiCallDetails == null || !response.ApiCallDetails.HasSuccessfulStatusCode)
            {
                var code = response.ApiCallDetails != null ? response.ApiCallDetails.HttpStatusCode : null;
                throw new InvalidOperationException("Search failed: " + code + " " + response.Body);
            }

            return JsonDocument.Parse(response.Body ?? "");
        }

        private static async Task ClosePitAsync(ElasticsearchClient client, string pitId, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(new { id = pitId });
            var response = await client.Transport.RequestAsync<StringResponse>(
                method: HttpMethod.DELETE,
                path: "/_pit",
                data: PostData.String(json),
                requestParameters: null!,
                cancellationToken: ct).ConfigureAwait(false);

            _ = response;
        }

        private void SetError(string message)
        {
            PostToUi(() =>
            {
                LastError = message;
                StatusText = "Error: " + message;
            });
        }

        private void PostToUi(Action action)
        {
            _uiContext.Post(_ => action(), null);
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
