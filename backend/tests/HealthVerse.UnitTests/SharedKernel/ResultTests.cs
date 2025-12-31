using FluentAssertions;
using HealthVerse.SharedKernel.Results;
using Xunit;

namespace HealthVerse.UnitTests.SharedKernel;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_WithValue_ShouldThrowWhenAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<int>(error);

        // Act & Assert
        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access value on failure result.");
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "test";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }
}
