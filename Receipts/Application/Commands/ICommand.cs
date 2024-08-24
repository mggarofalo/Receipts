using MediatR;

namespace Application.Common;

public interface ICommand<out TResult> : IRequest<TResult>
{
}
