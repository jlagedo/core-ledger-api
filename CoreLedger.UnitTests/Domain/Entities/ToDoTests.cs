using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;
using FluentAssertions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for ToDo domain entity business rules and invariants.
/// </summary>
public class ToDoTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidDescription_ShouldCreateToDo()
    {
        // Arrange
        var description = "Valid task description";

        // Act
        var todo = ToDo.Create(description);

        // Assert
        todo.Should().NotBeNull();
        todo.Description.Should().Be(description);
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
        todo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ShouldThrowDomainValidationException(string? description)
    {
        // Act
        var act = () => ToDo.Create(description);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Description cannot be empty");
    }

    [Fact]
    public void Create_WithDescriptionExceeding500Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var description = new string('x', 501);

        // Act
        var act = () => ToDo.Create(description);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Description cannot exceed 500 characters");
    }

    [Fact]
    public void Create_WithDescriptionExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var description = new string('x', 500);

        // Act
        var todo = ToDo.Create(description);

        // Assert
        todo.Should().NotBeNull();
        todo.Description.Should().HaveLength(500);
    }

    #endregion

    #region UpdateDescription Tests

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var todo = ToDo.Create("Original description");
        var originalCreatedAt = todo.CreatedAt;
        var newDescription = "Updated description";

        // Act
        todo.UpdateDescription(newDescription);

        // Assert
        todo.Description.Should().Be(newDescription);
        todo.UpdatedAt.Should().NotBeNull();
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todo.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_WithEmptyDescription_ShouldThrowDomainValidationException(string? description)
    {
        // Arrange
        var todo = ToDo.Create("Original description");

        // Act
        var act = () => todo.UpdateDescription(description);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Description cannot be empty");
    }

    [Fact]
    public void UpdateDescription_WithDescriptionExceeding500Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Original description");
        var description = new string('x', 501);

        // Act
        var act = () => todo.UpdateDescription(description);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Description cannot exceed 500 characters");
    }

    #endregion

    #region MarkAsCompleted Tests

    [Fact]
    public void MarkAsCompleted_WhenNotCompleted_ShouldSetIsCompletedAndCompletedAt()
    {
        // Arrange
        var todo = ToDo.Create("Test task");

        // Act
        todo.MarkAsCompleted();

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().NotBeNull();
        todo.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todo.UpdatedAt.Should().NotBeNull();
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsCompleted_WhenAlreadyCompleted_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Test task");
        todo.MarkAsCompleted();

        // Act
        var act = () => todo.MarkAsCompleted();

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("ToDo is already completed");
    }

    #endregion

    #region MarkAsIncomplete Tests

    [Fact]
    public void MarkAsIncomplete_WhenCompleted_ShouldClearIsCompletedAndCompletedAt()
    {
        // Arrange
        var todo = ToDo.Create("Test task");
        todo.MarkAsCompleted();

        // Act
        todo.MarkAsIncomplete();

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
        todo.UpdatedAt.Should().NotBeNull();
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsIncomplete_WhenNotCompleted_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Test task");

        // Act
        var act = () => todo.MarkAsIncomplete();

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("ToDo is already incomplete");
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void StateMachine_CompleteThenIncompleteThenComplete_ShouldWorkCorrectly()
    {
        // Arrange
        var todo = ToDo.Create("Test task");

        // Act & Assert - Complete
        todo.MarkAsCompleted();
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().NotBeNull();

        // Act & Assert - Incomplete
        todo.MarkAsIncomplete();
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();

        // Act & Assert - Complete again
        todo.MarkAsCompleted();
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().NotBeNull();
    }

    #endregion
}
