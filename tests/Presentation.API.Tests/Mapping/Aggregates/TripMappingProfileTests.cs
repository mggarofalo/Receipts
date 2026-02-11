using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;
using API.Mapping.Aggregates;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class TripMappingProfileTests
{
	private readonly IMapper _mapper;

	public TripMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TripMappingProfile>();
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		}, NullLoggerFactory.Instance);

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTripToTripVM()
	{
		// Arrange
		Trip expected = TripGenerator.Generate();

		// Act
		TripVM mapped = _mapper.Map<TripVM>(expected);
		Trip actual = _mapper.Map<Trip>(mapped);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void ShouldMapTripVMToTrip()
	{
		// Arrange
		TripVM expected = TripVMGenerator.Generate();

		// Act
		Trip mapped = _mapper.Map<Trip>(expected);
		TripVM actual = _mapper.Map<TripVM>(mapped);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}