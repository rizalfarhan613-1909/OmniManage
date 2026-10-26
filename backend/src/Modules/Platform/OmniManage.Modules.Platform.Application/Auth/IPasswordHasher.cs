namespace OmniManage.Modules.Platform.Application.Auth;

public interface IPasswordHasher
{
    string HashPassword(string password);

    PasswordVerificationOutcome VerifyHashedPassword(string passwordHash, string providedPassword);

    void PerformDummyVerification(string providedPassword);
}
