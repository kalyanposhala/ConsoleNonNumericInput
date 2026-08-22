namespace KMSS.Api.Domain.Enums;

public enum UserRole
{
    Admin,
    Member,
}

// The ledger tracks two loan products with separate columns throughout
// (సాధారణ అప్పు / ప్రత్యేక అప్పు). Only Ordinary is exposed in the v1 UI;
// Special exists so it isn't a schema migration later.
public enum LoanType
{
    Ordinary,
    Special,
}

public enum LoanStatus
{
    Active,
    Closed,
}

public enum ContributionStatus
{
    Pending,
    Paid,
    Partial,
}

// Every money movement is recorded as one of these. This is the enum behind
// the append-only Transaction ledger (see Transaction.cs) and lines up with
// the rows of the Sangam's "జమా ఖర్చుల పట్టిక" (receipts & payments) sheet.
public enum TransactionType
{
    // Receipts
    ShareContribution,
    OrdinaryLoanPrincipalRepayment,
    OrdinaryLoanInterestRepayment,
    SpecialLoanPrincipalRepayment,
    SpecialLoanInterestRepayment,
    Fine,
    ExtraInstallment,
    FormSale,
    OtherIncome,

    // Expenditures
    OrdinaryLoanDisbursement,
    SpecialLoanDisbursement,
    Wages,
    Stationery,
    OtherExpense,
}
