using FluentAssertions;
using HealthVerse.SharedKernel.Results;
using Xunit;

namespace HealthVerse.UnitTests.SharedKernel;

public class ErrorTests
{
    [Fact]
    public void None_ShouldHaveEmptyCodeAndMessage()
    {
        // Assert
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
    }

    [Fact]
    public void NullValue_ShouldHaveCorrectCodeAndMessage()
    {
        // Assert
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Message.Should().Be("A null value was provided.");
    }

    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange
        var message = "Invalid input";

        // Act
        var error = Error.Validation(message);

        // Assert
        error.Code.Should().Be("Error.Validation");
        error.Message.Should().Be(message);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange
        var entity = "User";

        // Act
        var error = Error.NotFound(entity);

        // Assert
        error.Code.Should().Be("Error.NotFound");
        error.Message.Should().Be("User was not found.");
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange
        var message = "Resource already exists";

        // Act
        var error = Error.Conflict(message);

        // Assert
        error.Code.Should().Be("Error.Conflict");
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange
        var message = "Access denied";

        // Act
        var error = Error.Forbidden(message);

        // Assert
        error.Code.Should().Be("Error.Forbidden");
        error.Message.Should().Be(message);
    }

    [Fact]
    public void Error_ShouldBeEqualWhenCodeAndMessageMatch()
    {
        // Arrange
        var error1 = new Error("Code", "Message");
        var error2 = new Error("Code", "Message");

        // Assert
        error1.Should().Be(error2);
    }

    [Fact]
    public void Error_ShouldNotBeEqualWhenCodeDiffers()
    {
        // Arrange
        var error1 = new Error("Code1", "Message");
        var error2 = new Error("Code2", "Message");

        // Assert
        error1.Should().NotBe(error2);
    }
}
