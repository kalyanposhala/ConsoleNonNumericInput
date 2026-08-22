using KMSS.Api.Domain.Enums;

namespace KMSS.Api.Domain.Entities;

// Login identity, kept separate from Member: every Member will normally have
// one User, but auth concerns (credentials, role) shouldn't live on the
// financial domain entity.
public class User
{
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
