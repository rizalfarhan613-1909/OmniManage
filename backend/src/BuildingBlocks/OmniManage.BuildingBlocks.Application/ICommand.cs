using MediatR;
using OmniManage.BuildingBlocks.Domain;

namespace OmniManage.BuildingBlocks.Application;

public interface ICommand : IRequest<Result>
{
}
