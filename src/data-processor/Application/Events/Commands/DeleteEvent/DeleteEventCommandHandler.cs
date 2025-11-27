using DataProcessor.Application.Common.Interfaces;
using MediatR;

namespace DataProcessor.Application.Events.Commands.DeleteEvent;

internal class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, bool>
{
    private readonly IEventRepository _repository;

    public DeleteEventCommandHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        => await _repository.DeleteAsync(request.Id, cancellationToken);
}



