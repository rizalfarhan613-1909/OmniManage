using OmniManage.BuildingBlocks.Application;
using OmniManage.BuildingBlocks.Domain;
using OmniManage.Modules.Platform.Domain.Abstractions;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Domain.Identity;
using OmniManage.Modules.Platform.Domain.Tenancy;

namespace OmniManage.Modules.Platform.Application.Auth;

public sealed class LoginCommandHandler(
    IUserRepository users,
    ITenantRepository tenants,
    IPasswordHasher passwordHasher,
    AuthSessionBuilder sessionBuilder,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenGenerator refreshTokenGenerator,
    IGuidGenerator guidGenerator,
    RefreshTokenOptions refreshTokenOptions,
    TimeProvider clock)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tenantCode = command.TenantCode?.Trim().ToUpperInvariant();
        var email = command.Email?.Trim().ToLowerInvariant();
        var password = command.Password;

        if (string.IsNullOrWhiteSpace(tenantCode)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            passwordHasher.PerformDummyVerification(password ?? string.Empty);
            return AuthErrors.InvalidCredentials;
        }

        var tenant = await tenants.GetByCodeAsync(tenantCode, cancellationToken).ConfigureAwait(false);

        if (tenant is null || tenant.Status != TenantStatus.Active)
        {
            passwordHasher.PerformDummyVerification(password);
            return AuthErrors.InvalidCredentials;
        }

        var user = await users
            .GetByEmailAsync(tenant.Id, email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !user.IsActive)
        {
            passwordHasher.PerformDummyVerification(password);
            return AuthErrors.InvalidCredentials;
        }

        var verification = passwordHasher.VerifyHashedPassword(user.PasswordHash, password);

        if (verification == PasswordVerificationOutcome.Failed)
        {
            return AuthErrors.InvalidCredentials;
        }

        if (verification == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            var rehash = user.ChangePasswordHash(passwordHasher.HashPassword(password));

            if (rehash.IsFailure)
            {
                return rehash.Error;
            }

            await users.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var session = await sessionBuilder
            .BuildAsync(tenant.Id, user.Id, cancellationToken)
            .ConfigureAwait(false);

        if (session.IsFailure)
        {
            return session.Error;
        }

        var sessionId = guidGenerator.NewGuid();
        var accessToken = tokenGenerator.Generate(
            user.Id,
            tenant.Id,
            session.Value.User.Roles,
            sessionId);

        var refreshTokenValue = refreshTokenGenerator.Generate();
        var issuedAt = clock.GetUtcNow();
        var expiresAt = issuedAt.AddDays(refreshTokenOptions.LifetimeDays);
        var refreshToken = RefreshToken.Issue(
            guidGenerator.NewGuid(),
            tenant.Id,
            user.Id,
            sessionId,
            refreshTokenValue.Hash,
            command.DeviceId,
            command.IpAddress,
            command.UserAgent,
            issuedAt,
            expiresAt);

        if (refreshToken.IsFailure)
        {
            return refreshToken.Error;
        }

        await refreshTokens.AddAsync(refreshToken.Value, cancellationToken).ConfigureAwait(false);
        await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new LoginResponse(
            accessToken.Token,
            "Bearer",
            accessToken.ExpiresAt,
            session.Value,
            refreshTokenValue.Token,
            expiresAt);
    }
}
