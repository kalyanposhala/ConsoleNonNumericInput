using KMSS.Api.Domain.Enums;

namespace KMSS.Api.Domain.Entities;

public class Loan
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public LoanType LoanType { get; set; } = LoanType.Ordinary;

    // Recorded per the ledger's guarantor register (page "అప్పుకోరువారి పేరు
    // మరియు జమానతుదారుల పేరు"). Required by business rule; nullable at the
    // DB level so a data-quality gap doesn't block a migration.
    public Guid? GuarantorMemberId { get; set; }
    public Member? GuarantorMember { get; set; }

    public decimal PrincipalDisbursed { get; set; }

    // Denormalized current balance for fast reads. The source of truth is
    // the LoanRepayment history — this field must always equal
    // PrincipalDisbursed minus the sum of non-reversed PrincipalPaid, and is
    // only ever updated in the same transaction as a repayment/correction.
    public decimal OutstandingPrincipal { get; set; }

    public decimal InterestRatePercent { get; set; } = 1.00m;

    public DateOnly DisbursedOn { get; set; }
    public DateOnly? ClosedOn { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}
