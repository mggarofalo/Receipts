using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Repositories;

public static class RepositoryHelpers
{
	public static IMapper CreateMapper<TProfile>() where TProfile : Profile, new()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TProfile>();
		}, NullLoggerFactory.Instance);

		configuration.AssertConfigurationIsValid();

		return configuration.CreateMapper();
	}
}
