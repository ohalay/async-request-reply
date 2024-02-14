public static class JobRouteBuilderExtension
{
    public static RouteGroupBuilder MapJobsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}", (string id, Store store, HttpResponse httpResponse) =>
        {
          if (!store.Jobs.TryGetValue(id, out var job) || job is null) 
              return Results.NotFound();

          if (job.Status == JobStatus.Completed && !string.IsNullOrEmpty(job.Location)) 
              return Results.Redirect(job.Location);

          if (job.Status == JobStatus.InProgress)
              httpResponse.Headers.RetryAfter = TimeSpan.FromSeconds(5).ToString();

          return Results.Ok(job);
        });

        return group;
    }
}
