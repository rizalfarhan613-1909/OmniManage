using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.Api.Seeding;

/// <summary>
/// Unwraps domain factory results during seeding. A failed factory call means
/// the fixture data is wrong, so failing fast is preferable to a partial seed.
/// </summary>
internal static class SeedGuard
{
    public static T EnsureSuccess<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The demo seed could not be created: {result.Error.Code}.");
        }

        return result.Value;
    }

    public static void EnsureSuccess(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"The demo seed could not be created: {result.Error.Code}.");
        }
    }
}
