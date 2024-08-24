using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Interfaces;

public interface IInfrastructureService
{
	IServiceCollection AddInfrastructureServices(IServiceCollection services, IConfiguration configuration);
}