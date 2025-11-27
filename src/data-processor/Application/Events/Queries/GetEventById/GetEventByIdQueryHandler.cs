using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Dtos;
using MediatR;

namespace DataProcessor.Application.Events.Queries.GetEventById;

internal class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IEventRepository _repository;

    public GetEventByIdQueryHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return record is null ? null : new EventDto(record.Id, record.CreatedAt, record.Value);
    }
}



