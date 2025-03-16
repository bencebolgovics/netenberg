namespace Netenberg.Model.Models;

public sealed record DatabaseOptions
{
    public required string DatabaseName { get; init; }
    public required string ConnectionString { get; init; }
}
