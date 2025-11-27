using DataProcessor.Application.Events.Dtos;
using MediatR;

namespace DataProcessor.Application.Events.Queries.GetEventById;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;



