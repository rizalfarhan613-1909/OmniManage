using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OmniManage.Api.Middleware;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Domain.Errors;
using OmniManage.Modules.Platform.Infrastructure.Auth;
using OmniManage.Modules.Platform.Infrastructure.Authorization;

namespace OmniManage.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddPlatformAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<JwtSigningKey, IOptions<JwtOptions>>((options, signingKey, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey.Key,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    NameClaimType = "sub",
                    RoleClaimType = "roles",
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async challengeContext =>
                    {
                        challengeContext.HandleResponse();

                        var writer = challengeContext.HttpContext.RequestServices
                            .GetRequiredService<IApiResponseWriter>();
                        var error = challengeContext.AuthenticateFailure
                            is SecurityTokenExpiredException
                                ? AuthErrors.TokenExpired
                                : AuthErrors.Unauthenticated;

                        await writer.WriteErrorAsync(
                                challengeContext.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                error.Code,
                                error.Message,
                                cancellationToken: challengeContext.HttpContext.RequestAborted)
                            .ConfigureAwait(false);
                    },
                    OnForbidden = async forbiddenContext =>
                    {
                        var writer = forbiddenContext.HttpContext.RequestServices
                            .GetRequiredService<IApiResponseWriter>();

                        await writer.WriteErrorAsync(
                                forbiddenContext.HttpContext,
                                StatusCodes.Status403Forbidden,
                                AuthErrors.Forbidden.Code,
                                AuthErrors.Forbidden.Message,
                                cancellationToken: forbiddenContext.HttpContext.RequestAborted)
                            .ConfigureAwait(false);
                    },
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddPlatformAuthorization();

        return services;
    }
}
