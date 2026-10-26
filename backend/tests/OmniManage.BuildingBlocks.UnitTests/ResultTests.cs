using FluentAssertions;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Success_WhenCreated_ReturnsSuccessWithNoError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WhenCreated_ReturnsFailureWithError()
    {
        var error = new Error("SALES.ORDER.NOT_FOUND", "The sales order was not found.", ErrorType.NotFound);

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WhenErrorIsNone_ThrowsInvalidOperationException()
    {
        var act = () => Result.Failure(Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenericSuccess_WhenCreated_ExposesValue()
    {
        var result = Result<string>.Success("order-1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("order-1");
    }

    [Fact]
    public void GenericFailure_WhenValueAccessed_ThrowsInvalidOperationException()
    {
        var error = new Error("FIN.INVOICE.NOT_FOUND", "The invoice was not found.", ErrorType.NotFound);

        var result = Result<string>.Failure(error);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromError_ReturnsFailedResult()
    {
        var error = new Error("SALES.ORDER.INVALID_STATE", "The order cannot be confirmed.", ErrorType.Conflict);

        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ReturnsSuccessfulResult()
    {
        Result<string> result = "order-1";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("order-1");
    }
}
