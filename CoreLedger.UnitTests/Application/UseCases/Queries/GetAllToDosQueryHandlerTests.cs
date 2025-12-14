using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.UseCases.ToDos.Queries;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CoreLedger.UnitTests.Application.UseCases.Queries;

/// <summary>
/// Unit tests for GetAllToDosQueryHandler.
/// </summary>
public class GetAllToDosQueryHandlerTests
{
    private readonly IToDoRepository _mockRepository;
    private readonly IMapper _mockMapper;
    private readonly ILogger<GetAllToDosQueryHandler> _mockLogger;
    private readonly GetAllToDosQueryHandler _handler;

    public GetAllToDosQueryHandlerTests()
    {
        _mockRepository = Substitute.For<IToDoRepository>();
        _mockMapper = Substitute.For<IMapper>();
        _mockLogger = Substitute.For<ILogger<GetAllToDosQueryHandler>>();
        _handler = new GetAllToDosQueryHandler(_mockRepository, _mockMapper, _mockLogger);
    }

    [Fact]
    public async Task Handle_WhenToDosExist_ShouldReturnMappedDtos()
    {
        // Arrange
        var query = new GetAllToDosQuery();
        var todos = new List<ToDo>
        {
            ToDo.Create("Task 1"),
            ToDo.Create("Task 2"),
            ToDo.Create("Task 3")
        };

        var expectedDtos = new List<ToDoDto>
        {
            new ToDoDto(1, "Task 1", false, DateTime.UtcNow, null),
            new ToDoDto(2, "Task 2", false, DateTime.UtcNow, null),
            new ToDoDto(3, "Task 3", false, DateTime.UtcNow, null)
        };

        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(todos);

        _mockMapper.Map<IReadOnlyList<ToDoDto>>(Arg.Any<IEnumerable<ToDo>>())
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedDtos);

        await _mockRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        _mockMapper.Received(1).Map<IReadOnlyList<ToDoDto>>(Arg.Any<IEnumerable<ToDo>>());
    }

    [Fact]
    public async Task Handle_WhenNoToDosExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllToDosQuery();
        var emptyList = new List<ToDo>();
        var emptyDtoList = new List<ToDoDto>();

        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(emptyList);

        _mockMapper.Map<IEnumerable<ToDoDto>>(Arg.Any<IEnumerable<ToDo>>())
            .Returns(emptyDtoList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        await _mockRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogRetrievalAndCount()
    {
        // Arrange
        var query = new GetAllToDosQuery();
        var todos = new List<ToDo>
        {
            ToDo.Create("Task 1"),
            ToDo.Create("Task 2")
        };

        var expectedDtos = new List<ToDoDto>
        {
            new ToDoDto(1, "Task 1", false, DateTime.UtcNow, null),
            new ToDoDto(2, "Task 2", false, DateTime.UtcNow, null)
        };

        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(todos);

        _mockMapper.Map<IReadOnlyList<ToDoDto>>(Arg.Any<IEnumerable<ToDo>>())
            .Returns(expectedDtos);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("all")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("2")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var query = new GetAllToDosQuery();
        var cancellationToken = new CancellationToken();
        var todos = new List<ToDo>();
        var dtos = new List<ToDoDto>();

        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(todos);

        _mockMapper.Map<IEnumerable<ToDoDto>>(Arg.Any<IEnumerable<ToDo>>())
            .Returns(dtos);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        await _mockRepository.Received(1).GetAllAsync(
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }
}
