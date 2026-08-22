using KMSS.Api.Domain.Enums;

namespace KMSS.Api.Domain.Entities;

// One row per member per month — the ₹200 "share" (వాటా ధనం) column in the ledger.
public class Contribution
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }

    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; } // 1-12

    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public DateOnly? PaidOn { get; set; }
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;

    public string? Notes { get; set; }
}
