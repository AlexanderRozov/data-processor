using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Commands.DeleteEvent;
using FluentAssertions;
using Moq;
using Xunit;

namespace DataProcessor.UnitTests;

public class DeleteEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_Returns_Result_From_Repository()
    {
        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteEventCommandHandler(repository.Object);
        var result = await handler.Handle(new DeleteEventCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeTrue();
    }
}

