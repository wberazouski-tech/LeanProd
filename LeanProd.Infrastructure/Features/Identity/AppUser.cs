using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Identity;

namespace LeanProd.Infrastructure.Features.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PreferredLanguage { get; set; } = SupportedLanguages.Belarusian;
    public Guid? DefaultDepartmentId { get; set; }
    public Guid? DefaultStorageLocationId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
