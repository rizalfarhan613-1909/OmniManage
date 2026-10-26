using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OmniManage.Modules.Platform.Application.Auth;
using OmniManage.Modules.Platform.Infrastructure.Auth;

namespace OmniManage.Modules.Platform.UnitTests.Auth;

public sealed class Argon2PasswordHasherTests
{
    private const string Password = "CorrectHorseBatteryStaple!";

    [Fact]
    public void HashPassword_UsesArgon2idWithMandatedParameters()
    {
        var hasher = new Argon2PasswordHasher();

        var hash = hasher.HashPassword(Password);

        hash.Should().StartWith("$argon2id$v=19$m=65536,t=3,p=2$");
        hash.Split('$').Should().HaveCount(6);
    }

    [Fact]
    public void VerifyHashedPassword_WithCorrectPassword_ReturnsSuccess()
    {
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.HashPassword(Password);

        var outcome = hasher.VerifyHashedPassword(hash, Password);

        outcome.Should().Be(PasswordVerificationOutcome.Success);
    }

    [Fact]
    public void VerifyHashedPassword_WithWrongPassword_ReturnsFailed()
    {
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.HashPassword(Password);

        var outcome = hasher.VerifyHashedPassword(hash, "WrongPassword123!");

        outcome.Should().Be(PasswordVerificationOutcome.Failed);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ProducesUniqueSalts()
    {
        var hasher = new Argon2PasswordHasher();

        var first = hasher.HashPassword(Password);
        var second = hasher.HashPassword(Password);

        first.Should().NotBe(second);
        hasher.VerifyHashedPassword(first, Password).Should().Be(PasswordVerificationOutcome.Success);
        hasher.VerifyHashedPassword(second, Password).Should().Be(PasswordVerificationOutcome.Success);
    }

    [Fact]
    public void VerifyHashedPassword_WithLegacyIdentityHash_RequestsRehash()
    {
        var hasher = new Argon2PasswordHasher();
        var legacyHasher = new PasswordHasher<object>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 210_000,
            }));
        var legacyHash = legacyHasher.HashPassword(new object(), Password);

        var outcome = hasher.VerifyHashedPassword(legacyHash, Password);

        outcome.Should().Be(PasswordVerificationOutcome.SuccessRehashNeeded);
    }

    [Fact]
    public void VerifyHashedPassword_WithTamperedHash_ReturnsFailed()
    {
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.HashPassword(Password);
        var parts = hash.Split('$');
        var digest = Convert.FromBase64String(parts[5].PadRight(parts[5].Length + ((4 - parts[5].Length % 4) % 4), '='));
        digest[0] ^= 0xFF;
        parts[5] = Convert.ToBase64String(digest).TrimEnd('=');
        var tampered = string.Join('$', parts);

        var outcome = hasher.VerifyHashedPassword(tampered, Password);

        outcome.Should().Be(PasswordVerificationOutcome.Failed);
    }

    [Fact]
    public void PerformDummyVerification_DoesNotThrow()
    {
        var hasher = new Argon2PasswordHasher();

        var action = () => hasher.PerformDummyVerification(Password);

        action.Should().NotThrow();
    }

    [Fact]
    public void PerformDummyVerification_WithEmptyPassword_DoesNotThrow()
    {
        var hasher = new Argon2PasswordHasher();

        var action = () => hasher.PerformDummyVerification(string.Empty);

        action.Should().NotThrow();
    }

    [Fact]
    public void VerifyHashedPassword_WithEmptyPassword_ReturnsFailed()
    {
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.HashPassword(Password);

        var outcome = hasher.VerifyHashedPassword(hash, string.Empty);

        outcome.Should().Be(PasswordVerificationOutcome.Failed);
    }

    [Fact]
    public void VerifyHashedPassword_WithMalformedHash_ReturnsFailedWithoutThrowing()
    {
        var hasher = new Argon2PasswordHasher();

        var action = () => hasher.VerifyHashedPassword("$argon2id$v=19$broken", Password);

        action.Should().NotThrow();
        action().Should().Be(PasswordVerificationOutcome.Failed);
    }
}
