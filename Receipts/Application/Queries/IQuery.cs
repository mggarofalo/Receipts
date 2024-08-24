using MediatR;

namespace Application.Queries;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
