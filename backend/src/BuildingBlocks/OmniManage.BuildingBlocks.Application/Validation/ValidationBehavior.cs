using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Application.Validation;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> before the command or
/// query handler. A failed validation short-circuits the pipeline with a
/// <see cref="Result"/> failure so handlers never receive invalid input.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var applicableValidators = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (applicableValidators.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
                applicableValidators.Select(validator =>
                    validator.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        return CreateFailure(failures);
    }

    private static TResponse CreateFailure(IReadOnlyCollection<ValidationFailure> failures)
    {
        var error = new Error(
            CommonErrors.ValidationFailed.Code,
            failures.First().ErrorMessage,
            ErrorType.Validation);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(TResponse).GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(Error)],
                modifiers: null);

            if (failureMethod?.Invoke(null, [error]) is TResponse failure)
            {
                return failure;
            }
        }

        throw new ValidationException(failures);
    }
}
