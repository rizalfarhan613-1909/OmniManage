namespace OmniManage.Modules.Finance.Domain.Gl;

/// <summary>
/// Attribution for postings produced by automatic posting rules.
/// </summary>
public static class FinanceSystemActor
{
    // ASSUMPTION: Platform does not yet expose a system principal for automated
    // work. Automatic postings are attributed to this reserved identifier until
    // the platform realm provides one.
    public static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
