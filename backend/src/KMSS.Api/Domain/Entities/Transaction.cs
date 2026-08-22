using KMSS.Api.Domain.Enums;

namespace KMSS.Api.Domain.Entities;

// Append-only general ledger: every rupee that moves into or out of the
// common fund is recorded here exactly once, regardless of which
// domain table (Contribution, Loan, LoanRepayment) it also updates.
// The monthly financial summary (జమా ఖర్చుల పట్టిక) is a computed
// aggregation over this table grouped by PeriodYear/PeriodMonth/Type —
// it is never hand-maintained.
public class Transaction
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }

    // Positive = inflow to the common fund, negative = outflow.
    public decimal Amount { get; set; }

    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public DateOnly TransactionDate { get; set; }

    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public Guid? LoanId { get; set; }
    public Loan? Loan { get; set; }

    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsReversed { get; set; }
    public Guid? ReversalOfTransactionId { get; set; }
    public string? Notes { get; set; }
}
