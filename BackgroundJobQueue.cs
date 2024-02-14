using System.Threading.Channels;

public class BackgroundJobQueue
{
    private readonly Channel<(string id, Func<CancellationToken, ValueTask>)> queue;

    public BackgroundJobQueue(int capacity = 10)
    {
        var options = new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait };
        queue = Channel.CreateBounded<(string id, Func<CancellationToken, ValueTask>)>(options);
    }

    public async ValueTask QueueJobAsync(string id, Func<CancellationToken, ValueTask> workItem)
        => await queue.Writer.WriteAsync((id, workItem));
    public async ValueTask<(string id, Func<CancellationToken, ValueTask> job)> DequeueJobAsync(CancellationToken cancellationToken)
        => await queue.Reader.ReadAsync(cancellationToken);
}

public class QueuedHostedService : BackgroundService
{
    private readonly BackgroundJobQueue queue;
    private readonly Store store;

    public QueuedHostedService(BackgroundJobQueue jobQueue, Store store)
        => (queue, this.store) = (jobQueue, store);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await BackgroundProcessing(stoppingToken);

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
          var (id, jobTask) = await queue.DequeueJobAsync(stoppingToken);
        if (!store.Jobs.TryGetValue(id, out var job))
          return;

        try
        {
          await jobTask(stoppingToken);
          job = job with { CompletedAt = DateTime.UtcNow, Status = JobStatus.Completed };
        }
        catch (Exception ex)
        {
          job = job with { CompletedAt = DateTime.UtcNow, Error = ex.Message, Status = JobStatus.Failed };
        }

        store.Jobs[id] = job;
      }
    }
}
