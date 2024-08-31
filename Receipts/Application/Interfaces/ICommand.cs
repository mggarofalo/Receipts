using MediatR;

namespace Application.Interfaces;

public interface ICommand<out TResult> : IRequest<TResult>
{
}
