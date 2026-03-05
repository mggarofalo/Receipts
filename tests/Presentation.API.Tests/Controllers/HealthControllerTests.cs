using API.Controllers;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class HealthControllerTests
{
	[Fact]
	public async Task Get_WhenDatabaseIsHealthy_ReturnsOk()
	{
		Mock<DatabaseFacade> dbFacadeMock = new(Mock.Of<DbContext>());
		dbFacadeMock.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		Mock<ApplicationDbContext> dbContextMock = new(
			new DbContextOptionsBuilder<ApplicationDbContext>().Options);
		dbContextMock.Setup(c => c.Database).Returns(dbFacadeMock.Object);

		HealthController controller = new();

		IResult result = await controller.Get(dbContextMock.Object, CancellationToken.None);

		Ok<object> okResult = Assert.IsType<Ok<object>>(result);
		Assert.NotNull(okResult.Value);
	}

	[Fact]
	public async Task Get_WhenDatabaseIsUnhealthy_Returns503()
	{
		Mock<DatabaseFacade> dbFacadeMock = new(Mock.Of<DbContext>());
		dbFacadeMock.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		Mock<ApplicationDbContext> dbContextMock = new(
			new DbContextOptionsBuilder<ApplicationDbContext>().Options);
		dbContextMock.Setup(c => c.Database).Returns(dbFacadeMock.Object);

		HealthController controller = new();

		IResult result = await controller.Get(dbContextMock.Object, CancellationToken.None);

		Assert.IsNotType<Ok<object>>(result);
	}
}
