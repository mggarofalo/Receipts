using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

[Mapper]
public partial class AccountMapper
{
	public partial AccountVM ToViewModel(Account source);
	public partial Account ToDomain(AccountVM source);
}
