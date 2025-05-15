namespace Netenberg.Model.Options;

public sealed record DatabaseOptions
{
    public required string DatabaseName { get; init; }
    public required string ConnectionString { get; init; }
}
