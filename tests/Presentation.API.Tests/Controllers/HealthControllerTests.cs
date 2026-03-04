using API.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Presentation.API.Tests.Controllers;

public class HealthControllerTests
{
	[Fact]
	public void Get_ReturnsHealthy()
	{
		HealthController controller = new();

		Ok<object> result = controller.Get();

		Assert.NotNull(result.Value);
	}
}
