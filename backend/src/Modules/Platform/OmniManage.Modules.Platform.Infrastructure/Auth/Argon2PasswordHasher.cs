using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OmniManage.Modules.Platform.Application.Auth;

namespace OmniManage.Modules.Platform.Infrastructure.Auth;

/// <summary>
/// Argon2id password hashing as required by Security.md §2.2
/// (memory ≥ 64 MiB, iterations ≥ 3, parallelism ≥ 2).
///
/// Hashes are stored as PHC strings so the parameters travel with the hash and
/// can be upgraded transparently on the next successful login. Legacy
/// ASP.NET Core Identity PBKDF2 hashes remain verifiable during migration and
/// report <see cref="PasswordVerificationOutcome.SuccessRehashNeeded"/>.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    public const int MemorySizeKb = 65_536;
    public const int Iterations = 3;
    public const int DegreeOfParallelism = 2;

    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MinimumHashSize = 16;
    private const int MaximumHashSize = 64;
    private const int MaximumMemorySizeKb = 1_048_576;
    private const int MaximumIterations = 10;
    private const int MaximumDegreeOfParallelism = 16;
    private const string Algorithm = "argon2id";
    private const string AlgorithmVersion = "v=19";
    private const string DummyPassword = "omnimanage-dummy-password";

    private readonly object _legacyUser = new();
    private readonly PasswordHasher<object> _legacyHasher;
    private readonly Lazy<string> _dummyHash;

    public Argon2PasswordHasher()
    {
        _legacyHasher = new PasswordHasher<object>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 210_000,
            }));
        _dummyHash = new Lazy<string>(
            () => HashPassword(DummyPassword),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, MemorySizeKb, Iterations, DegreeOfParallelism, HashSize);

        return string.Join(
            '$',
            string.Empty,
            Algorithm,
            AlgorithmVersion,
            $"m={MemorySizeKb},t={Iterations},p={DegreeOfParallelism}",
            Encode(salt),
            Encode(hash));
    }

    public PasswordVerificationOutcome VerifyHashedPassword(string passwordHash, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(providedPassword);

        if (providedPassword.Length == 0)
        {
            // Argon2 rejects an empty password outright. Do equivalent dummy
            // work so a missing credential is indistinguishable from a wrong
            // one, then always fail.
            PerformDummyVerification(providedPassword);
            return PasswordVerificationOutcome.Failed;
        }

        if (passwordHash.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            return VerifyArgon2Id(passwordHash, providedPassword);
        }

        // Legacy ASP.NET Core Identity PBKDF2 hash created before the Argon2id
        // migration. A successful login rehashes with Argon2id.
        return _legacyHasher.VerifyHashedPassword(_legacyUser, passwordHash, providedPassword) switch
        {
            PasswordVerificationResult.Success => PasswordVerificationOutcome.SuccessRehashNeeded,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
            _ => PasswordVerificationOutcome.Failed,
        };
    }

    public void PerformDummyVerification(string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(providedPassword);

        // Argon2 rejects empty input; substitute a fixed value so the dummy
        // verification still costs the same as a real one.
        var candidate = providedPassword.Length == 0 ? DummyPassword : providedPassword;

        _ = VerifyHashedPassword(_dummyHash.Value, candidate);
    }

    private static PasswordVerificationOutcome VerifyArgon2Id(string encodedHash, string password)
    {
        if (!TryParse(encodedHash, out var parameters, out var salt, out var expectedHash))
        {
            return PasswordVerificationOutcome.Failed;
        }

        var actualHash = Derive(
            password,
            salt,
            parameters.MemorySizeKb,
            parameters.Iterations,
            parameters.Parallelism,
            expectedHash.Length);

        if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
        {
            return PasswordVerificationOutcome.Failed;
        }

        var needsRehash = parameters.MemorySizeKb < MemorySizeKb
            || parameters.Iterations < Iterations
            || parameters.Parallelism < DegreeOfParallelism;

        return needsRehash
            ? PasswordVerificationOutcome.SuccessRehashNeeded
            : PasswordVerificationOutcome.Success;
    }

    private static byte[] Derive(
        string password,
        byte[] salt,
        int memorySizeKb,
        int iterations,
        int parallelism,
        int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memorySizeKb,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };

        return argon2.GetBytes(hashSize);
    }

    private static bool TryParse(
        string encodedHash,
        out Argon2Parameters parameters,
        out byte[] salt,
        out byte[] hash)
    {
        parameters = default;
        salt = [];
        hash = [];

        var parts = encodedHash.Split('$');

        // ["", "argon2id", "v=19", "m=...,t=...,p=...", salt, hash]
        if (parts.Length != 6
            || !string.Equals(parts[1], Algorithm, StringComparison.Ordinal)
            || !string.Equals(parts[2], AlgorithmVersion, StringComparison.Ordinal))
        {
            return false;
        }

        var parsedParameters = ParseParameters(parts[3]);

        if (parsedParameters is null)
        {
            return false;
        }

        if (!TryDecode(parts[4], out salt) || !TryDecode(parts[5], out hash))
        {
            return false;
        }

        if (salt.Length < SaltSize
            || hash.Length is < MinimumHashSize or > MaximumHashSize)
        {
            return false;
        }

        parameters = parsedParameters.Value;
        return true;
    }

    private static Argon2Parameters? ParseParameters(string value)
    {
        var memorySizeKb = 0;
        var iterations = 0;
        var parallelism = 0;

        foreach (var pair in value.Split(','))
        {
            var segments = pair.Split('=');

            if (segments.Length != 2
                || !int.TryParse(segments[1], out var parsed)
                || parsed <= 0)
            {
                return null;
            }

            switch (segments[0])
            {
                case "m":
                    memorySizeKb = parsed;
                    break;
                case "t":
                    iterations = parsed;
                    break;
                case "p":
                    parallelism = parsed;
                    break;
                default:
                    return null;
            }
        }

        if (memorySizeKb is < 1 or > MaximumMemorySizeKb
            || iterations is < 1 or > MaximumIterations
            || parallelism is < 1 or > MaximumDegreeOfParallelism)
        {
            return null;
        }

        return new Argon2Parameters(memorySizeKb, iterations, parallelism);
    }

    private static string Encode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=');

    private static bool TryDecode(string value, out byte[] bytes)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');

        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded,
        };

        try
        {
            bytes = Convert.FromBase64String(padded);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private readonly record struct Argon2Parameters(
        int MemorySizeKb,
        int Iterations,
        int Parallelism);
}
