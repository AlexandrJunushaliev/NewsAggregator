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
        ILogger<SearchIndexBase<Dictionary<string, SearchIndexSortedSet>>> baseLogger) : base(baseLogger,
        new Dictionary<string, SearchIndexSortedSet>())
    {
        _logger = logger;
    }

    public async Task AddToIndex(Dictionary<string, HashSet<SearchIndexEntry>> newEntries,
        HashSet<string>? wordsToDelete)
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
                if (wordsToDelete is null || !wordsToDelete.Contains(kvp.Key))
                {
                    newIndex[kvp.Key] = new SearchIndexSortedSet();
                    foreach (var entry in kvp.Value)
                    {
                        newIndex[kvp.Key].Add(entry.Id);
                    }
                }
            }

            foreach (var kvp in Index)
            {
                if (wordsToDelete is null || !wordsToDelete.Contains(kvp.Key))
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

    public IEnumerable<SearchIndexEntryId> Find(IEnumerable<string> keyWords, int take, int skip, bool fromOlder,
        DateTime? leftBorder, DateTime? rightBorder)
    {
        Dictionary<string, SearchIndexSortedSet>? index;
        lock (_locker)
        {
            index = Index;
        }

        try
        {
            return IterateEntriesInOrder(keyWords, index, fromOlder, leftBorder, rightBorder).Skip(skip).Take(take);
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception happen on search request {e}");
            return Array.Empty<SearchIndexEntryId>();
        }
    }

    public int Count(IEnumerable<string> keyWords,
        DateTime? leftBorder, DateTime? rightBorder)
    {
        Dictionary<string, SearchIndexSortedSet>? index;
        lock (_locker)
        {
            index = Index;
        }

        try
        {
            return IterateEntriesInOrder(keyWords, index, false, leftBorder, rightBorder).Count();
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception happen on search request {e}");
            return default;
        }
    }

    private static IEnumerable<SearchIndexEntryId> IterateEntriesInOrder(IEnumerable<string> keywords,
        Dictionary<string, SearchIndexSortedSet> index, bool fromOlder, DateTime? left, DateTime? right)
    {
        var firstElements = keywords
            .Where(index.ContainsKey)
            .Select(x =>
                left != null && right != null
                    ? index[x].GetViewBetween(
                        new SearchIndexEntryId(string.Empty, right.Value),
                        new SearchIndexEntryId(string.Empty, left.Value))
                    : index[x])
            .Select(x => !fromOlder ? x : x.Reverse())
            .Where(x => x.Any())
            .Select(x => x.GetEnumerator())
            .ToArray();
        var finished = new HashSet<int>();
        foreach (var firstElement in firstElements)
        {
            firstElement.MoveNext();
        }

        SearchIndexEntryId lastReturned = default;
        var olderDefault = new SearchIndexEntryId(string.Empty, DateTime.MaxValue);
        while (true)
        {
            if (finished.Count == firstElements.Length)
                yield break;
            var maxI = 0;
            var maxId = fromOlder ? olderDefault : default;

            for (var i = 0; i < firstElements.Length; i++)
            {
                if (finished.Contains(i))
                    continue;
                var curr = firstElements[i].Current;
                if (fromOlder ? curr < maxId : curr > maxId)
                {
                    maxId = curr;
                    maxI = i;
                }
            }

            if (!firstElements[maxI].MoveNext())
            {
                finished.Add(maxI);
            }

            if (lastReturned == maxId) continue;
            lastReturned = maxId;
            yield return maxId;
        }
    }

    public void LoadSnapshot() => LoadSnapshot(_locker);

    private readonly object _locker = new object();

    protected override string GetSnapshotFileName() => "reverse_index_snapshot";

    private readonly SemaphoreSlim _semaphore = new(1);
}