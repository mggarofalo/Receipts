using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public static class ControllerTestHelpers
{
	public static Mock<ILogger<T>> GetLoggerMock<T>()
	{
		Mock<ILogger<T>> loggerMock = new();

		loggerMock.Setup(x => x.Log(
			LogLevel.Error,
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>())).Verifiable();
		loggerMock.Setup(x => x.Log(
			LogLevel.Critical,
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).Verifiable();

		return loggerMock;
	}
}
