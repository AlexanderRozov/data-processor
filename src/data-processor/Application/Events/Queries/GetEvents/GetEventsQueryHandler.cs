using System.Linq;
using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Dtos;
using MediatR;

namespace DataProcessor.Application.Events.Queries.GetEvents;

internal class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IReadOnlyCollection<EventDto>>
{
    private readonly IEventRepository _repository;

    public GetEventsQueryHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAsync(request.From, request.To, cancellationToken);
        return items.Select(record => new EventDto(record.Id, record.CreatedAt, record.Value)).ToArray();
    }
}

