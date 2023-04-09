using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NewsProcessor.Domain;

namespace NewsProcessor.Index;

public class ReverseSearchIndex : SearchIndexBase<Dictionary<string, SearchIndexSortedSet>>
{
    private readonly ILogger<ReverseSearchIndex> _logger;

    public ReverseSearchIndex(ILogger<ReverseSearchIndex> logger,
        ILogger<SearchIndexBase<Dictionary<string, SearchIndexSortedSet>>> baseLogger) : base(baseLogger, new Dictionary<string, SearchIndexSortedSet>())
    {
        _logger = logger;
    }

    public async Task AddToIndex(Dictionary<string, HashSet<SearchIndexEntry>> newEntries)
    {
        await _semaphore.WaitAsync();
        {
            _logger.LogInformation("Start adding to index");
            var sw = new Stopwatch();
            sw.Start();
            var entries = newEntries.SelectMany(x => x.Value).Select(x => x.Id).ToHashSet();
            var newIndex = new Dictionary<string, SearchIndexSortedSet>();
            foreach (var kvp in newEntries)
            {
                newIndex[kvp.Key] = new SearchIndexSortedSet();
                foreach (var entry in kvp.Value)
                {
                    newIndex[kvp.Key].Add(entry.Id);
                }
            }

            foreach (var kvp in Index)
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
                Index = newIndex;
            }

            _logger.LogInformation($"Adding finished in {sw.Elapsed}");
            await SaveSnapshot(_locker);
        }
        _semaphore.Release();
    }

    public HashSet<SearchIndexEntryId> Find(IEnumerable<string> keyWords, int take, int skip)
    {
        Dictionary<string, SearchIndexSortedSet>? index;
        lock (_locker)
        {
            index = Index;
        }

        var resultHashSet = new HashSet<SearchIndexEntryId>();
        foreach (var key in keyWords)
        {
            if (index.TryGetValue(key, out var sl))
            {
                foreach (var id in sl.Skip(skip).Take(take))
                {
                    resultHashSet.Add(id);
                }
            }
        }

        return resultHashSet;
    }

    public void LoadSnapshot() => LoadSnapshot(_locker);

    private readonly object _locker = new object();

    protected override string GetSnapshotFileName() => "reverse_index_snapshot";

    private readonly SemaphoreSlim _semaphore = new(1);
}