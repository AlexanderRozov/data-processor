using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Commands.CreateEvent;
using DataProcessor.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace DataProcessor.UnitTests;

public class CreateEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_Persists_Event_And_Returns_Dto()
    {
        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.InsertAsync(It.IsAny<EventRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateEventCommandHandler(repository.Object);
        var command = new CreateEventCommand(Guid.NewGuid(), DateTime.UtcNow, 42);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(42);
        repository.Verify(r => r.InsertAsync(It.Is<EventRecord>(e => e.Id == command.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}

