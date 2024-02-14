public static class TodoRouteBuilderExtension
{
    public static RouteGroupBuilder MapTodosApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", (Store store) => store.Todos.Values.ToArray());
        group.MapGet("/{id}", (string id, Store store) =>
            store.Todos.TryGetValue(id, out var todo)
                ? Results.Ok(todo)
                : Results.NotFound());

        group.MapPost("/", async (Todo model, BackgroundJobQueue queue, Store store, HttpResponse response) =>
        {
            model = model with { Id = Guid.NewGuid().ToString() };
            var job = new Job(Guid.NewGuid().ToString(), DateTime.UtcNow, JobStatus.InProgress, $"/api/todos/{model.Id}");

            store.Jobs.Add(job.Id, job);

            Func<CancellationToken, ValueTask> workItem = async (token) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), token);
                store.Todos.Add(model.Id, model);

            };
            await queue.QueueJobAsync(job.Id, workItem);

            response.Headers.RetryAfter = TimeSpan.FromSeconds(2).ToString();
            response.Headers.Location = $"/api/jobs/{job.Id}";
            response.StatusCode = StatusCodes.Status202Accepted;
        });

        return group;
    }
}