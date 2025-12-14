using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Commands;

/// <summary>
/// Unit tests for UpdateToDoCommandHandler.
/// </summary>
public class UpdateToDoCommandHandlerTests
{
    private readonly IToDoRepository _mockRepository;
    private readonly ILogger<UpdateToDoCommandHandler> _mockLogger;
    private readonly UpdateToDoCommandHandler _handler;

    public UpdateToDoCommandHandlerTests()
    {
        _mockRepository = Substitute.For<IToDoRepository>();
        _mockLogger = Substitute.For<ILogger<UpdateToDoCommandHandler>>();
        _handler = new UpdateToDoCommandHandler(_mockRepository, _mockLogger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateToDoAndReturnDto()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, "Updated description", true);

        var existingToDo = ToDo.Create("Original description");

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingToDo);

        _mockRepository.UpdateAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockRepository.Received(1).UpdateAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenToDoNotFound_ShouldReturnNull()
    {
        // Arrange
        var command = new UpdateToDoCommand(999, "Updated description", false);

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((ToDo?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        await _mockRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMarkingAsCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, "Task to complete", true);

        var existingToDo = ToDo.Create("Task to complete");

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingToDo);

        _mockRepository.UpdateAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).UpdateAsync(
            Arg.Is<ToDo>(t => t.IsCompleted && t.CompletedAt != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMarkingAsIncomplete_ShouldClearCompletedAt()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, "Completed task", false);

        var existingToDo = ToDo.Create("Completed task");
        existingToDo.MarkAsCompleted();

        _mockRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingToDo);

        _mockRepository.UpdateAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).UpdateAsync(
            Arg.Is<ToDo>(t => !t.IsCompleted && t.CompletedAt == null),
            Arg.Any<CancellationToken>());
    }
}
