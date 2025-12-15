using System.Diagnostics;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

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
        Assert.NotNull(todo);
        Assert.Equal(description, todo.Description);
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAt);
        Assert.True((DateTime.UtcNow - todo.CreatedAt).TotalSeconds < 1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ShouldThrowDomainValidationException(string? description)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
        {
            Debug.Assert(description != null, nameof(description) + " != null");
            return ToDo.Create(description);
        });
        Assert.Equal("Description cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_WithDescriptionExceeding500Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var description = new string('x', 501);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => ToDo.Create(description));
        Assert.Equal("Description cannot exceed 500 characters", exception.Message);
    }

    [Fact]
    public void Create_WithDescriptionExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var description = new string('x', 500);

        // Act
        var todo = ToDo.Create(description);

        // Assert
        Assert.NotNull(todo);
        Assert.Equal(500, todo.Description.Length);
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
        Assert.Equal(newDescription, todo.Description);
        Assert.NotNull(todo.UpdatedAt);
        Assert.True((DateTime.UtcNow - todo.UpdatedAt.Value).TotalSeconds < 1);
        Assert.Equal(originalCreatedAt, todo.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_WithEmptyDescription_ShouldThrowDomainValidationException(string? description)
    {
        // Arrange
        var todo = ToDo.Create("Original description");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
        {
            Debug.Assert(description != null, nameof(description) + " != null");
            todo.UpdateDescription(description);
        });
        Assert.Equal("Description cannot be empty", exception.Message);
    }

    [Fact]
    public void UpdateDescription_WithDescriptionExceeding500Characters_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Original description");
        var description = new string('x', 501);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => todo.UpdateDescription(description));
        Assert.Equal("Description cannot exceed 500 characters", exception.Message);
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
        Assert.True(todo.IsCompleted);
        Assert.NotNull(todo.CompletedAt);
        Assert.True((DateTime.UtcNow - todo.CompletedAt.Value).TotalSeconds < 1);
        Assert.NotNull(todo.UpdatedAt);
        Assert.True((DateTime.UtcNow - todo.UpdatedAt.Value).TotalSeconds < 1);
    }

    [Fact]
    public void MarkAsCompleted_WhenAlreadyCompleted_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Test task");
        todo.MarkAsCompleted();

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => todo.MarkAsCompleted());
        Assert.Equal("ToDo is already completed", exception.Message);
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
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAt);
        Assert.NotNull(todo.UpdatedAt);
        Assert.True((DateTime.UtcNow - todo.UpdatedAt.Value).TotalSeconds < 1);
    }

    [Fact]
    public void MarkAsIncomplete_WhenNotCompleted_ShouldThrowDomainValidationException()
    {
        // Arrange
        var todo = ToDo.Create("Test task");

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => todo.MarkAsIncomplete());
        Assert.Equal("ToDo is already incomplete", exception.Message);
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
        Assert.True(todo.IsCompleted);
        Assert.NotNull(todo.CompletedAt);

        // Act & Assert - Incomplete
        todo.MarkAsIncomplete();
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAt);

        // Act & Assert - Complete again
        todo.MarkAsCompleted();
        Assert.True(todo.IsCompleted);
        Assert.NotNull(todo.CompletedAt);
    }

    #endregion
}
