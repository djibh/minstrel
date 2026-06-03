namespace Minstrel.Application.Sources;

public record PCloudAuthResult(bool IsConnected, bool RequiresEmailCode)
{
    public static PCloudAuthResult Success() => new(true, false);
    public static PCloudAuthResult CodeRequired() => new(false, true);
}
