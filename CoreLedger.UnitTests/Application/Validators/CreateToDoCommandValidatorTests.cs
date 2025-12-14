using CoreLedger.Application.UseCases.ToDos.Commands;
using CoreLedger.Application.Validators;

namespace CoreLedger.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for CreateToDoCommandValidator.
/// Tests FluentValidation rules for CreateToDoCommand.
/// </summary>
public class CreateToDoCommandValidatorTests
{
    private readonly CreateToDoCommandValidator _validator;

    public CreateToDoCommandValidatorTests()
    {
        _validator = new CreateToDoCommandValidator();
    }

    [Fact]
    public void Validate_WithValidDescription_ShouldPass()
    {
        // Arrange
        var command = new CreateToDoCommand("Valid task description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDescription_ShouldFail(string? description)
    {
        // Arrange
        var command = new CreateToDoCommand(description!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateToDoCommand.Description), result.Errors[0].PropertyName);
        Assert.Contains("required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithDescriptionExceeding500Characters_ShouldFail()
    {
        // Arrange
        var command = new CreateToDoCommand(new string('x', 501));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateToDoCommand.Description), result.Errors[0].PropertyName);
        Assert.Contains("500 characters", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithDescriptionExactly500Characters_ShouldPass()
    {
        // Arrange
        var command = new CreateToDoCommand(new string('x', 500));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(499)]
    public void Validate_WithDescriptionWithinValidRange_ShouldPass(int length)
    {
        // Arrange
        var command = new CreateToDoCommand(new string('x', length));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
