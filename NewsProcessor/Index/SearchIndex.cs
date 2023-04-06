using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NewsProcessor.Domain;

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
                var entries = newEntries.SelectMany(x => x.Value).Select(x=>x.Id).ToHashSet();
                var newIndex = new Dictionary<string, SearchIndexSortedSet>();
                foreach (var kvp in newEntries)
                {
                    newIndex[kvp.Key] = new SearchIndexSortedSet();
                    foreach (var entry in kvp.Value)
                    {
                        newIndex[kvp.Key].Add(entry.Id);
                    }
                }

                foreach (var kvp in _index)
                {
                    if (!newIndex.ContainsKey(kvp.Key))
                    {
                        newIndex[kvp.Key] = new SearchIndexSortedSet();
                    }

                    foreach (var entry in kvp.Value)
                    {
                        if (!entries.Contains(entry))
                        {
                            if (!newIndex[kvp.Key].Contains(entry))
                                newIndex[kvp.Key].Add(entry);
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

    public (DateTime lastProcessedTime, HashSet<SearchIndexEntryId> entries) Find(IEnumerable<string> keyWords, int take,
        int skip)
    {
        Dictionary<string, SearchIndexSortedSet>? index;
        DateTime lastTimeProcessed;
        lock (_locker)
        {
            index = _index;
            lastTimeProcessed = _lastProcessedTime;
        }

        var resultHashSet = new HashSet<SearchIndexEntryId>();
        foreach (var key in keyWords)
        {
            var sl = index[key];
            foreach (var id in sl.Skip(skip).Take(take))
            {
                resultHashSet.Add(id);
            }
        }

        return (lastTimeProcessed, resultHashSet);
    }

    private async Task SaveSnapshot()
    {
        _logger.LogInformation($"Start saving snapshot");
        var sw = new Stopwatch();
        Dictionary<string, SearchIndexSortedSet>? index;
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
            JsonSerializer.Deserialize<Dictionary<string, SearchIndexSortedSet>>(
                File.Open(SnapshotLocation, FileMode.Open), Options
            );
        lock (_locker)
        {
            _index = newInd!;
        }

        GC.Collect(3, GCCollectionMode.Forced);
        _logger.LogInformation($"Snapshot loaded in {sw.Elapsed}");
    }

    private Dictionary<string, SearchIndexSortedSet> _index = new();
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