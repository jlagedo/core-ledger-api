using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Application.Validators;
using FluentAssertions;

namespace CoreLedger.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for UpdateToDoCommandValidator.
/// Tests FluentValidation rules for UpdateToDoCommand.
/// </summary>
public class UpdateToDoCommandValidatorTests
{
    private readonly UpdateToDoCommandValidator _validator;

    public UpdateToDoCommandValidatorTests()
    {
        _validator = new UpdateToDoCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, "Valid updated description", false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Validate_WithInvalidId_ShouldFail(int id)
    {
        // Arrange
        var command = new UpdateToDoCommand(id, "Valid description", false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateToDoCommand.Id));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("greater than"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDescription_ShouldFail(string? description)
    {
        // Arrange
        var command = new UpdateToDoCommand(1, description!, false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateToDoCommand.Description));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WithDescriptionExceeding500Characters_ShouldFail()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, new string('x', 501), false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateToDoCommand.Description));
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("500 characters"));
    }

    [Fact]
    public void Validate_WithDescriptionExactly500Characters_ShouldPass()
    {
        // Arrange
        var command = new UpdateToDoCommand(1, new string('x', 500), false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WithDifferentIsCompletedValues_ShouldPass(bool isCompleted)
    {
        // Arrange
        var command = new UpdateToDoCommand(1, "Valid description", isCompleted);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new UpdateToDoCommand(0, new string('x', 501), false);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateToDoCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateToDoCommand.Description));
    }
}
