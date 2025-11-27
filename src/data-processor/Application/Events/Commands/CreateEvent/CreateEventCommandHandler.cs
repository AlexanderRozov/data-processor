using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Dtos;
using DataProcessor.Domain;
using MediatR;

namespace DataProcessor.Application.Events.Commands.CreateEvent;

internal class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IEventRepository _repository;

    public CreateEventCommandHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var record = new EventRecord(request.Id, request.CreatedAt, request.Value);
        await _repository.InsertAsync(record, cancellationToken);
        return new EventDto(record.Id, record.CreatedAt, record.Value);
    }
}



