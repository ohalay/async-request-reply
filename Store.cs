using System.Text.Json.Serialization;

public record Todo(string Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
public record Job(string Id, DateTime CreatedAt, JobStatus Status, string Location, DateTime? CompletedAt = null, string Error = null);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatus { InProgress = 0, Completed, Failed }

public class Store
{
    public Dictionary<string, Todo> Todos { get; } = new Dictionary<string, Todo>();
    public Dictionary<string, Job> Jobs { get; } = new Dictionary<string, Job>();
}
