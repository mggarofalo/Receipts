using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using Shared.Mapping.Aggregates;
using Shared.Mapping.Core;

namespace Presentation.Shared.Tests.Mapping.Aggregates;

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
		});

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTripToTripVM()
	{
		// Arrange
		Trip original = TripGenerator.Generate();

		// Act
		TripVM mapped = _mapper.Map<TripVM>(original);
		Trip reverseMapped = _mapper.Map<Trip>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}

	[Fact]
	public void ShouldMapTripVMToTrip()
	{
		// Arrange
		TripVM original = TripVMGenerator.Generate();

		// Act
		Trip mapped = _mapper.Map<Trip>(original);
		TripVM reverseMapped = _mapper.Map<TripVM>(mapped);

		// Assert
		Assert.Equal(original, reverseMapped);
	}
}