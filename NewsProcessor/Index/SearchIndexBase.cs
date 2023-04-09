using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NewsProcessor.Domain;

namespace NewsProcessor.Index;

public abstract class SearchIndexBase<T>
{
    private readonly ILogger<SearchIndexBase<T>> _logger;

    protected SearchIndexBase(ILogger<SearchIndexBase<T>> logger, T initValue)
    {
        _logger = logger;
        Index = initValue;
    }

    protected async Task SaveSnapshot(object locker)
    {
        _logger.LogInformation($"Start saving snapshot");
        var sw = new Stopwatch();
        sw.Start();
        T? index;
        lock (locker)
        {
            index = Index;
        }

        if (!Directory.Exists(SnapshotDir))
            Directory.CreateDirectory(SnapshotDir);

        await using (var fs = File.Create(Path.Combine(SnapshotDir, GetSnapshotFileName()) + ".tmp"))
        {
            await JsonSerializer.SerializeAsync(fs, index, Options);
        }

        File.Move(Path.Combine(SnapshotDir, GetSnapshotFileName()) + ".tmp", Path.Combine(SnapshotDir, GetSnapshotFileName()), true);
        _logger.LogInformation($"Snapshot saved in {sw.Elapsed}");
    }

    protected void LoadSnapshot(object locker)
    {
        if (!File.Exists(Path.Combine(SnapshotDir, GetSnapshotFileName())))
        {
            return;
        }

        _logger.LogInformation($"Start loading snapshot");
        var sw = new Stopwatch();
        sw.Start();
        var newInd =
            JsonSerializer.Deserialize<T>(
                File.Open(Path.Combine(SnapshotDir, GetSnapshotFileName()), FileMode.Open), Options
            );
        lock (locker)
        {
            Index = newInd!;
        }

        GC.Collect(3, GCCollectionMode.Forced);
        _logger.LogInformation($"Snapshot loaded in {sw.Elapsed}");
    }

    protected T Index;
    private const string SnapshotDir = "snapshots";
    protected abstract string GetSnapshotFileName();
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new SearchIndexEntryIdJsonConverter() }, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
}