namespace OmniManage.Modules.Finance.Domain.Gl;

/// <summary>
/// Control accounts used by the automatic posting rules. A tenant's chart of
/// accounts is provisioned lazily with these defaults on first posting so that
/// UC-01 never dead-letters because of missing setup data.
/// </summary>
public static class ChartOfAccounts
{
    public const string CashAndBankCode = "1010";
    public const string CashAndBankName = "Cash/Bank";
    public const string AccountsReceivableCode = "1100";
    public const string AccountsReceivableName = "Accounts Receivable";
    public const string TaxPayableCode = "2100";
    public const string TaxPayableName = "Tax Payable";
    public const string PayrollDeductionsPayableCode = "2200";
    public const string PayrollDeductionsPayableName = "Payroll Deductions Payable";
    public const string SalesRevenueCode = "4000";
    public const string SalesRevenueName = "Sales Revenue";
    public const string PayrollExpenseCode = "5000";
    public const string PayrollExpenseName = "Payroll Expense";
}
