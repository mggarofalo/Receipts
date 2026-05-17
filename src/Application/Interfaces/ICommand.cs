using Mediator;

namespace Application.Interfaces;

public interface ICommand<out TResult> : IRequest<TResult>
{
}
