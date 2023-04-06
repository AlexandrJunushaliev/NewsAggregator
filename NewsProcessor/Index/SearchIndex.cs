using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using NewsProcessor.Domain;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NewsProcessor.Index;

public class SearchIndex
{
    private readonly ILogger<SearchIndex> _logger;

    public SearchIndex(ILogger<SearchIndex> logger)
    {
        _logger = logger;
    }

    public async Task AddToIndex(Dictionary<string, HashSet<SearchIndexEntry>> newEntries)
    {
        await _semaphore.WaitAsync();
        {
            try
            {
                var sw = new Stopwatch();
                _logger.LogInformation("Start adding to index");
                var entries = newEntries.SelectMany(x => x.Value).ToHashSet();
                var newIndex = new Dictionary<string, SortedList<SearchIndexEntryId, SearchIndexEntry>>();
                foreach (var kvp in newEntries)
                {
                    newIndex[kvp.Key] = new SortedList<SearchIndexEntryId, SearchIndexEntry>();
                    foreach (var entry in kvp.Value)
                    {
                        newIndex[kvp.Key].Add(entry.Id, default);
                    }
                }

                foreach (var kvp in _index)
                {
                    if (!newIndex.ContainsKey(kvp.Key))
                    {
                        newIndex[kvp.Key] = new SortedList<SearchIndexEntryId, SearchIndexEntry>();
                    }

                    foreach (var entry in kvp.Value)
                    {
                        if (!entries.Contains(entry.Value))
                        {
                            if (!newIndex[kvp.Key].ContainsKey(entry.Key))
                                newIndex[kvp.Key].Add(entry.Key, entry.Value);
                        }
                    }
                }

                lock (_locker)
                {
                    _index = newIndex;
                    _lastProcessedTime = DateTime.UtcNow;
                }

                _logger.LogInformation($"Adding finished in {sw.Elapsed}");
                await SaveSnapshot();
            }
            catch
            {
            }
        }
        _semaphore.Release();
    }

    public (DateTime lastProcessedTime, HashSet<SearchIndexEntry> entries) Find(IEnumerable<string> keyWords, int take,
        int skip)
    {
        Dictionary<string, SortedList<SearchIndexEntryId, SearchIndexEntry>>? index;
        DateTime lastTimeProcessed;
        lock (_locker)
        {
            index = _index;
            lastTimeProcessed = _lastProcessedTime;
        }

        var resultHashSet = new HashSet<SearchIndexEntry>();
        foreach (var key in keyWords)
        {
            var sl = index[key];
            if (skip > sl.Count)
            {
                continue;
            }

            if (take + skip >= sl.Count)
            {
                for (var i = sl.Count - 1 - skip; i >= 0; i--)
                {
                    resultHashSet.Add(sl.Values[i]);
                }

                continue;
            }

            for (var i = sl.Count - 1 - skip; i > sl.Count - 1 - skip - take; i--)
            {
                resultHashSet.Add(sl.Values[i]);
            }
        }

        return (lastTimeProcessed, resultHashSet);
    }

    private async Task SaveSnapshot()
    {
        _logger.LogInformation($"Start saving snapshot");
        var sw = new Stopwatch();
        Dictionary<string, SortedList<SearchIndexEntryId, SearchIndexEntry>>? index;
        lock (_locker)
        {
            index = _index;
        }

        if (!Directory.Exists(SnapshotDir))
            Directory.CreateDirectory(SnapshotDir);

        await using (var fs = File.Create(SnapshotLocation + ".tmp"))
        {
            await JsonSerializer.SerializeAsync(fs, index, Options);
        }

        File.Move(SnapshotLocation + ".tmp", SnapshotLocation, true);
        _logger.LogInformation($"Snapshot saved in {sw.Elapsed}");
    }

    public void LoadSnapshot()
    {
        if (!File.Exists(SnapshotLocation))
        {
            return;
        }

        _logger.LogInformation($"Start loading snapshot");
        var sw = new Stopwatch();
        var newInd =
            JsonSerializer.Deserialize<Dictionary<string, SortedList<SearchIndexEntryId, SearchIndexEntry>>>(
                File.Open(SnapshotLocation, FileMode.Open), Options
            );
        lock (_locker)
        {
            _index = newInd!;
        }

        GC.Collect(3, GCCollectionMode.Forced);
        _logger.LogInformation($"Snapshot loaded in {sw.Elapsed}");
    }

    private Dictionary<string, SortedList<SearchIndexEntryId, SearchIndexEntry>> _index = new();
    private object _locker = new();
    private DateTime _lastProcessedTime = DateTime.MinValue;
    private readonly SemaphoreSlim _semaphore = new(1);
    private const string SnapshotDir = "snapshots";
    private static readonly string SnapshotLocation = Path.Combine(SnapshotDir, "snapshot");

    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new SearchIndexEntryIdJsonConverter() }, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
}