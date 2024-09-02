using API.Mapping.Aggregates;
using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;

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
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTripToTripVMAndBackWithoutLosingData()
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
	public void ShouldMapTripVMToTripAndBackWithoutLosingData()
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