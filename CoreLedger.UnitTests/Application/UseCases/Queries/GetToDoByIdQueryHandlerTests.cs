using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.ToDos.Queries;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using CoreLedger.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Queries;

/// <summary>
/// Unit tests for GetToDoByIdQueryHandler.
/// </summary>
public class GetToDoByIdQueryHandlerTests
{
    private readonly IToDoRepository _mockRepository;
    private readonly IMapper _mockMapper;
    private readonly ILogger<GetToDoByIdQueryHandler> _mockLogger;
    private readonly GetToDoByIdQueryHandler _handler;

    public GetToDoByIdQueryHandlerTests()
    {
        _mockRepository = Substitute.For<IToDoRepository>();
        _mockMapper = Substitute.For<IMapper>();
        _mockLogger = Substitute.For<ILogger<GetToDoByIdQueryHandler>>();
        _handler = new GetToDoByIdQueryHandler(_mockRepository, _mockMapper, _mockLogger);
    }

    [Fact]
    public async Task Handle_WhenToDoExists_ShouldReturnMappedDto()
    {
        // Arrange
        var query = new GetToDoByIdQuery(1);
        var todo = ToDo.Create("Existing task");
        var expectedDto = new ToDoDto(
            Id: 1,
            Description: "Existing task",
            IsCompleted: false,
            CreatedAt: DateTime.UtcNow,
            CompletedAt: null
        );

        _mockRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(todo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        await _mockRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
        _mockMapper.Received(1).Map<ToDoDto>(Arg.Any<ToDo>());
    }

    [Fact]
    public async Task Handle_WhenToDoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetToDoByIdQuery(999);

        _mockRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((ToDo?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        await _mockRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
        _mockMapper.DidNotReceive().Map<ToDoDto>(Arg.Any<ToDo>());
    }

    [Fact]
    public async Task Handle_WhenToDoExists_ShouldLogRetrieval()
    {
        // Arrange
        var query = new GetToDoByIdQuery(1);
        var todo = ToDo.Create("Task with logging");
        var expectedDto = new ToDoDto(1, "Task with logging", false, DateTime.UtcNow, null);

        _mockRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(todo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(expectedDto);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"{query.Id}")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenToDoNotFound_ShouldLogWarningAndThrow()
    {
        // Arrange
        var query = new GetToDoByIdQuery(999);

        _mockRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((ToDo?)null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
        
        _mockLogger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"{query.Id}")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var query = new GetToDoByIdQuery(1);
        var cancellationToken = new CancellationToken();
        var todo = ToDo.Create("Task");
        var dto = new ToDoDto(1, "Task", false, DateTime.UtcNow, null);

        _mockRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(todo);

        _mockMapper.Map<ToDoDto>(Arg.Any<ToDo>())
            .Returns(dto);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        await _mockRepository.Received(1).GetByIdAsync(
            query.Id,
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }
}
