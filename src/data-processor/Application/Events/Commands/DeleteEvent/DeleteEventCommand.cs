using MediatR;

namespace DataProcessor.Application.Events.Commands.DeleteEvent;

public sealed record DeleteEventCommand(Guid Id) : IRequest<bool>;



