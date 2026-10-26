using MediatR;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Application;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
