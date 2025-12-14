using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Commands;

/// <summary>
/// Unit tests for DeleteToDoCommandHandler.
/// </summary>
public class DeleteToDoCommandHandlerTests
{
    private readonly IToDoRepository _mockRepository;
    private readonly ILogger<DeleteToDoCommandHandler> _mockLogger;
    private readonly DeleteToDoCommandHandler _handler;

    public DeleteToDoCommandHandlerTests()
    {
        _mockRepository = Substitute.For<IToDoRepository>();
        _mockLogger = Substitute.For<ILogger<DeleteToDoCommandHandler>>();
        _handler = new DeleteToDoCommandHandler(_mockRepository, _mockLogger);
    }

    [Fact]
    public async Task Handle_WithExistingToDo_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var command = new DeleteToDoCommand(1);
        var existingToDo = ToDo.Create("Task to delete");

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingToDo);

        _mockRepository.DeleteAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockRepository.Received(1).DeleteAsync(
            Arg.Is<ToDo>(t => t == existingToDo),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenToDoNotFound_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeleteToDoCommand(999);

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((ToDo?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        await _mockRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var command = new DeleteToDoCommand(1);
        var existingToDo = ToDo.Create("Task to delete");
        var cancellationToken = new CancellationToken();

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingToDo);

        _mockRepository.DeleteAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        await _mockRepository.Received(1).GetByIdAsync(
            command.Id,
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));

        await _mockRepository.Received(1).DeleteAsync(
            Arg.Any<ToDo>(),
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }
}
