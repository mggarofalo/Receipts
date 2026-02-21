using Application.Interfaces.Services;
using System.Security.Claims;

namespace API.Services;

public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
	public string? UserId =>
		httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

	public Guid? ApiKeyId
	{
		get
		{
			string? value = httpContextAccessor.HttpContext?.User.FindFirst("ApiKeyId")?.Value;
			return value is not null && Guid.TryParse(value, out Guid id) ? id : null;
		}
	}

	public string? IpAddress =>
		httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

	public string? UserAgent =>
		httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
