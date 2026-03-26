namespace Minstrel.Domain.ValueObjects;

public class StreamDescriptor
{
    public required Uri StreamUri { get; init; }
    public bool IsRedirectPreferred { get; init; }
}
