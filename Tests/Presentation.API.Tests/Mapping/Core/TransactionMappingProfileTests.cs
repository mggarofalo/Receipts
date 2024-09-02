using API.Mapping.Core;
using AutoMapper;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class TransactionMappingProfileTests
{
	private readonly IMapper _mapper;

	public TransactionMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TransactionMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapTransactionToTransactionVM()
	{
		Transaction transaction = new(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			new Money(100),
			DateOnly.FromDateTime(DateTime.Now)
		);

		TransactionVM transactionVM = _mapper.Map<TransactionVM>(transaction);

		Assert.Equal(transaction.Id, transactionVM.Id);
		Assert.Equal(transaction.ReceiptId, transactionVM.ReceiptId);
		Assert.Equal(transaction.AccountId, transactionVM.AccountId);
		Assert.Equal(transaction.Amount.Amount, transactionVM.Amount);
		Assert.Equal(transaction.Date, transactionVM.Date);
	}

	[Fact]
	public void ShouldMapTransactionVMToTransaction()
	{
		TransactionVM transactionVM = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = Guid.NewGuid(),
			AccountId = Guid.NewGuid(),
			Amount = 100,
			Date = DateOnly.FromDateTime(DateTime.Now)
		};

		Transaction transaction = _mapper.Map<Transaction>(transactionVM);

		Assert.Equal(transactionVM.Id, transaction.Id);
		Assert.Equal(transactionVM.ReceiptId, transaction.ReceiptId);
		Assert.Equal(transactionVM.AccountId, transaction.AccountId);
		Assert.Equal(transactionVM.Amount, transaction.Amount.Amount);
		Assert.Equal(transactionVM.Date, transaction.Date);
	}
}