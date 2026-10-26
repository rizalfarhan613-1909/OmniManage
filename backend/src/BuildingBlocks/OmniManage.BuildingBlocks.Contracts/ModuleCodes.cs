using System.Collections.Frozen;

namespace OmniManage.BuildingBlocks.Contracts;

public static class ModuleCodes
{
    public const string PlatformCore = "PLATFORM_CORE";
    public const string CoreMasterData = "CORE_MASTERDATA";
    public const string FinGl = "FIN_GL";
    public const string FinAr = "FIN_AR";
    public const string FinAp = "FIN_AP";
    public const string ProcCore = "PROC_CORE";
    public const string SalesCore = "SALES_CORE";
    public const string InvCore = "INV_CORE";
    public const string MfgCore = "MFG_CORE";
    public const string HrisCore = "HRIS_CORE";
    public const string HrisAttendance = "HRIS_ATTENDANCE";
    public const string HrisPayroll = "HRIS_PAYROLL";
    public const string HrisEss = "HRIS_ESS";
    public const string CrmCore = "CRM_CORE";
    public const string AssetCore = "ASSET_CORE";
    public const string ProjectCore = "PROJECT_CORE";

    public static IReadOnlySet<string> All { get; } = new[]
    {
        PlatformCore,
        CoreMasterData,
        FinGl,
        FinAr,
        FinAp,
        ProcCore,
        SalesCore,
        InvCore,
        MfgCore,
        HrisCore,
        HrisAttendance,
        HrisPayroll,
        HrisEss,
        CrmCore,
        AssetCore,
        ProjectCore,
    }.ToFrozenSet(StringComparer.Ordinal);
}
