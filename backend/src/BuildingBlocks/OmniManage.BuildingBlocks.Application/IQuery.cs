using MediatR;

namespace OmniManage.BuildingBlocks.Application;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
