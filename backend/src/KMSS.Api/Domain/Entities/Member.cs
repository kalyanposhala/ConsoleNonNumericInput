namespace KMSS.Api.Domain.Entities;

public class Member
{
    public Guid Id { get; set; }

    // The Sangam's paper "ఖాతా నెం" (khata/book number) — kept as the
    // human-facing identifier members and the admin already recognize.
    public int KhataNo { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly JoinedOn { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
