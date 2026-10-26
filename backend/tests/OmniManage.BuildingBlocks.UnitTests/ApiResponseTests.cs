using FluentAssertions;
using OmniManage.BuildingBlocks.Contracts;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class ApiResponseTests
{
    private static readonly ApiMeta Meta = new(
        "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
        new DateTime(2026, 10, 21, 8, 15, 30, DateTimeKind.Utc),
        new PaginationMeta(1, 25, 134, 6));

    [Fact]
    public void Ok_WhenCalled_ReturnsSuccessEnvelope()
    {
        var response = ApiResponse<string>.Ok("order-1", Meta);

        response.Success.Should().BeTrue();
        response.Data.Should().Be("order-1");
        response.Error.Should().BeNull();
        response.Meta.Should().Be(Meta);
    }

    [Fact]
    public void Fail_WhenCalled_ReturnsErrorEnvelope()
    {
        var error = new ApiError(
            "VALIDATION.FAILED",
            "The request is invalid.",
            [new ApiErrorDetail("customerId", "REQUIRED", "The customer is required.")]);

        var response = ApiResponse<string>.Fail(error, Meta);

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().Be(error);
        response.Error!.Details.Should().ContainSingle();
    }
}
