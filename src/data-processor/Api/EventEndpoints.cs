using System.Linq;
using DataProcessor.Api.Models;
using DataProcessor.Application.Events.Commands.CreateEvent;
using DataProcessor.Application.Events.Commands.DeleteEvent;
using DataProcessor.Application.Events.Dtos;
using DataProcessor.Application.Events.Queries.GetEventById;
using DataProcessor.Application.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Routing;

namespace DataProcessor.Api;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/events")
            .WithTags("Events");

        group.MapGet("/", async (DateTime? from, DateTime? to, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetEventsQuery(from, to), ct);
            return Results.Ok(result.Select(ToResponse));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetEventByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(ToResponse(result));
        });

        group.MapPost("/", async (CreateEventRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateEventCommand(Guid.NewGuid(), DateTime.UtcNow, request.Value);
            var result = await mediator.Send(command, ct);
            return Results.Created($"/events/{result.Id}", ToResponse(result));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteEventCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return group;
    }

    private static EventResponse ToResponse(EventDto dto)
        => new(dto.Id, dto.CreatedAt, dto.Value);
}

