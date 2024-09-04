using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public static class ControllerTestHelpers
{
	public static Mock<IMapper> GetMapperMock<TSource, TDestination>(IMapper mapper)
	{
		Mock<IMapper> mapperMock = new();

		mapperMock.Setup(m => m.Map<TSource, TDestination>(It.IsAny<TSource>())).Returns<TSource>(source => mapper.Map<TDestination>(source));
		mapperMock.Setup(m => m.Map<TDestination, TSource>(It.IsAny<TDestination>())).Returns<TDestination>(destination => mapper.Map<TSource>(destination));

		return mapperMock;
	}

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

	public static void VerifyLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, int times)
	{
		loggerMock.Verify(logger => logger.Log(
			It.Is<LogLevel>(l => l == logLevel),
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Exactly(times));
	}

	public static void VerifyNoLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel)
	{
		loggerMock.Verify(logger => logger.Log(
			It.Is<LogLevel>(l => l == logLevel),
			It.IsAny<EventId>(),
			It.IsAny<It.IsAnyType>(),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Never());
	}

	public static void VerifyLoggingCallWithMessage<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string messageContained)
	{
		loggerMock.Verify(logger => logger.Log(
			It.Is<LogLevel>(l => l == logLevel),
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(messageContained)),
			It.IsAny<Exception>(),
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
	}

	public static void VerifyTraceLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, int times)
	{
		loggerMock.VerifyLoggingCalls(LogLevel.Trace, times);
	}

	public static void VerifyNoTraceLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Trace);
	}

	public static void VerifyDebugLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, int times)
	{
		loggerMock.VerifyLoggingCalls(LogLevel.Debug, times);
	}

	public static void VerifyNoDebugLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Debug);
	}

	public static void VerifyInformationLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, int times)
	{
		loggerMock.VerifyLoggingCalls(LogLevel.Information, times);
	}

	public static void VerifyNoInformationLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Information);
	}

	public static void VerifyWarningLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, int times)
	{
		loggerMock.VerifyLoggingCalls(LogLevel.Warning, times);
	}

	public static void VerifyNoWarningLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Warning);
	}

	public static void VerifyErrorLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, string messageContained)
	{
		loggerMock.VerifyLoggingCallWithMessage(LogLevel.Error, messageContained);
	}

	public static void VerifyNoErrorLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Error);
	}

	public static void VerifyCriticalLoggingCalls<T>(this Mock<ILogger<T>> loggerMock, string messageContained)
	{
		loggerMock.VerifyLoggingCallWithMessage(LogLevel.Critical, messageContained);
	}

	public static void VerifyNoCriticalLoggingCalls<T>(this Mock<ILogger<T>> loggerMock)
	{
		loggerMock.VerifyNoLoggingCalls(LogLevel.Critical);
	}
}