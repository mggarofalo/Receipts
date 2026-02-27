using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class AccountMapper
{
	[MapperIgnoreTarget(nameof(AccountEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(AccountEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(AccountEntity.DeletedByApiKeyId))]
	public partial AccountEntity ToEntity(Account source);

	[MapperIgnoreSource(nameof(AccountEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(AccountEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(AccountEntity.DeletedByApiKeyId))]
	public partial Account ToDomain(AccountEntity source);
}
