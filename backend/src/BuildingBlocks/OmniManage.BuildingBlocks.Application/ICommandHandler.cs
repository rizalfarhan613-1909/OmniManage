using MediatR;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Application;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}
