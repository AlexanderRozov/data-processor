using DataProcessor.Application.Events.Dtos;
using MediatR;

namespace DataProcessor.Application.Events.Queries.GetEvents;

public record GetEventsQuery(DateTime? From = null, DateTime? To = null) : IRequest<IReadOnlyCollection<EventDto>>;



