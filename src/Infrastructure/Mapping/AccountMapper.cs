using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class AccountMapper
{
	public partial AccountEntity ToEntity(Account source);
	public partial Account ToDomain(AccountEntity source);
}
