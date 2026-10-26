namespace OmniManage.BuildingBlocks.Infrastructure.Persistence;

public sealed class TenantIsolationException : Exception
{
    public TenantIsolationException()
        : base("The entity tenant does not match the active tenant context.")
    {
    }

    public TenantIsolationException(string message)
        : base(message)
    {
    }

    public TenantIsolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
