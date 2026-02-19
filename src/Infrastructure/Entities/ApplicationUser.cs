using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? RefreshToken { get; set; }
	public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
	public virtual ICollection<ApiKeyEntity> ApiKeys { get; set; } = [];
}
