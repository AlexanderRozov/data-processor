using DataProcessor.Application.Events.Dtos;
using MediatR;

namespace DataProcessor.Application.Events.Commands.CreateEvent;

public record CreateEventCommand(Guid Id, DateTime CreatedAt, int Value) : IRequest<EventDto>;



