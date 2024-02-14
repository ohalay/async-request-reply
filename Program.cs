using System;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton<Store>()
    .AddSingleton<BackgroundJobQueue>()
    .AddHostedService<QueuedHostedService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapGroup("api/todos")
    .MapTodosApi();

app.MapGroup("api/jobs")
    .MapJobsApi();

app.Run();


[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Job))]
[JsonSerializable(typeof(JobStatus))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
