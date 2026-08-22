namespace KMSS.Api.Domain.Entities;

// One row per repayment the admin records. Interest is always computed
// server-side as InterestRatePercent% of the loan's outstanding principal
// *before* this repayment is applied — never trusted from the client.
public class LoanRepayment
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Loan? Loan { get; set; }

    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }

    // Snapshot of the loan's outstanding principal immediately after this
    // repayment, so history stays reproducible even if later corrections
    // change how OutstandingPrincipal is derived.
    public decimal OutstandingPrincipalAfter { get; set; }

    public DateOnly PaidOn { get; set; }
    public Guid RecordedByUserId { get; set; }

    // Corrections never delete a row: a wrong entry is reversed by writing a
    // new offsetting repayment and flagging both, preserving the audit trail.
    public bool IsReversed { get; set; }
    public Guid? ReversalOfRepaymentId { get; set; }
}
