public static class JobRouteBuilderExtension
{
    public static RouteGroupBuilder MapJobsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}", (string id, Store store, HttpResponse httpResponse) =>
        {
            if (!store.Jobs.TryGetValue(id, out var job) || job is null)
                return Results.NotFound();


            var okResult = () =>
            {
                httpResponse.Headers.RetryAfter = TimeSpan.FromSeconds(5).ToString();
                return Results.Ok(job);
            };

            return job.Status switch
            {
                JobStatus.Completed => Results.Redirect(job.Location),
                JobStatus.InProgress => okResult(),
                JobStatus.Failed => Results.BadRequest(job),
                _ => throw new NotImplementedException(),
            };
        });

        return group;
    }
}
