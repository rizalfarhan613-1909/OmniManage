namespace OmniManage.BuildingBlocks.Application;

public sealed class LicenseSourceUnavailableException : Exception
{
    public LicenseSourceUnavailableException()
    {
    }

    public LicenseSourceUnavailableException(string message)
        : base(message)
    {
    }

    public LicenseSourceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
