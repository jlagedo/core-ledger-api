using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Commands;

/// <summary>
/// Unit tests for CreateToDoCommandHandler.
/// Tests command handling logic with mocked dependencies.
/// </summary>
public class CreateToDoCommandHandlerTests
{
    private readonly IToDoRepository _mockRepository;
    private readonly IMapper _mockMapper;
    private readonly ILogger<CreateToDoCommandHandler> _mockLogger;
    private readonly CreateToDoCommandHandler _handler;

    public CreateToDoCommandHandlerTests()
    {
        _mockRepository = Substitute.For<IToDoRepository>();
        _mockMapper = Substitute.For<IMapper>();
        _mockLogger = Substitute.For<ILogger<CreateToDoCommandHandler>>();
        _handler = new CreateToDoCommandHandler(_mockRepository, _mockMapper, _mockLogger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateToDoAndReturnDto()
    {
        // Arrange
        var command = new CreateToDoCommand("New task");
        var createdToDo = ToDo.Create(command.Description);
        var expectedDto = new ToDoDto(
            Id: 1,
            Description: command.Description,
            IsCompleted: false,
            CreatedAt: DateTime.UtcNow,
            CompletedAt: null
        );

        _mockRepository.AddAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(createdToDo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(command.Description);
        result.IsCompleted.Should().BeFalse();

        await _mockRepository.Received(1).AddAsync(
            Arg.Is<ToDo>(t => t.Description == command.Description && !t.IsCompleted),
            Arg.Any<CancellationToken>());

        _mockMapper.Received(1).Map<ToDoDto>(Arg.Any<ToDo>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetCreatedAtTimestamp()
    {
        // Arrange
        var command = new CreateToDoCommand("Task with timestamp");
        var expectedDto = new ToDoDto(
            Id: 1,
            Description: command.Description,
            IsCompleted: false,
            CreatedAt: DateTime.UtcNow,
            CompletedAt: null
        );

        var createdToDo = ToDo.Create(command.Description);
        
        _mockRepository.AddAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(createdToDo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(expectedDto);

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        await _mockRepository.Received(1).AddAsync(
            Arg.Is<ToDo>(t => t.CreatedAt >= beforeExecution && t.CreatedAt <= afterExecution),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var command = new CreateToDoCommand("Cancellable task");
        var cancellationToken = new CancellationToken();
        var createdToDo = ToDo.Create(command.Description);
        var expectedDto = new ToDoDto(1, command.Description, false, DateTime.UtcNow, null);

        _mockRepository.AddAsync(Arg.Any<ToDo>(), Arg.Any<CancellationToken>())
            .Returns(createdToDo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(expectedDto);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        await _mockRepository.Received(1).AddAsync(
            Arg.Any<ToDo>(),
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }
}
