namespace Utils;

public class Limiter
{
    private readonly SemaphoreSlim _semaphore;

    public Limiter(int limit)
    {
        _semaphore = new SemaphoreSlim(limit);
    }

    public async Task<T> Await<T>(Func<Task<T>> createTask)
    {
        try
        {
            await _semaphore.WaitAsync();
            return await createTask();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T[]> AwaitAll<T>(IEnumerable<Func<Task<T>>> tasks)
    {
        return await Task.WhenAll(tasks.Select(Await));
    }
}