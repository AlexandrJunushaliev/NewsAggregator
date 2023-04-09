using System.Diagnostics;
using NewsProcessor.Domain;

namespace NewsProcessor.Index;

public class SearchIndex : SearchIndexBase<Dictionary<SearchIndexEntryId, HashSet<string>>>
{
    private readonly ILogger<SearchIndex> _logger;

    public SearchIndex(ILogger<SearchIndex> logger,
        ILogger<SearchIndexBase<Dictionary<SearchIndexEntryId, HashSet<string>>>> baseLogger) : base(baseLogger,
        new Dictionary<SearchIndexEntryId, HashSet<string>>())
    {
        _logger = logger;
    }

    public async Task AddToIndex(Dictionary<SearchIndexEntry, HashSet<string>> newEntries)
    {
        await _semaphore.WaitAsync();
        {
            var sw = new Stopwatch();
            _logger.LogInformation("Start adding to index");
            var entries = newEntries.Keys.Select(x=>x.Id).ToArray();
            var newIndex = new Dictionary<SearchIndexEntryId, HashSet<string>>();
            foreach (var kvp in newEntries)
            {
                newIndex[kvp.Key.Id] = new HashSet<string>();
                foreach (var entry in kvp.Value)
                {
                    newIndex[kvp.Key.Id].Add(entry);
                }
            }

            foreach (var kvp in Index)
            {
                if (!entries.Contains(kvp.Key))
                {
                    newIndex.Add(kvp.Key, kvp.Value);
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

    public Dictionary<SearchIndexEntryId, HashSet<string>> FindKeyWords(IEnumerable<SearchIndexEntryId> news)
    {
        Dictionary<SearchIndexEntryId, HashSet<string>>? index;
        lock (_locker)
        {
            index = Index;
        }

        var result = new Dictionary<SearchIndexEntryId, HashSet<string>>();
        foreach (var key in news)
        {
            if (index.ContainsKey(key))
                result.Add(key, index[key]);
        }

        return result;
    }

    public void LoadSnapshot() => LoadSnapshot(_locker);

    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly object _locker = new object();

    protected override string GetSnapshotFileName() => "snapshot";
}