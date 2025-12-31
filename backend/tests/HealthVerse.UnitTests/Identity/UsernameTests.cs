using FluentAssertions;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Identity;

public class UsernameTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("testuser")]
    [InlineData("User123")]
    public void Create_WithValidUsername_ShouldSucceed(string username)
    {
        // Act
        var result = Username.Create(username);

        // Assert
        result.Value.Should().Be(username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNull_ShouldThrow(string? username)
    {
        // Act
        var act = () => Username.Create(username!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("x")]
    public void Create_WithTooShort_ShouldThrow(string username)
    {
        // Act
        var act = () => Username.Create(username);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*must be at least 3 characters*");
    }

    [Fact]
    public void Create_WithTooLong_ShouldThrow()
    {
        // Arrange
        var username = new string('x', 51);

        // Act
        var act = () => Username.Create(username);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    [Fact]
    public void Create_WithLeadingWhitespace_ShouldThrow()
    {
        // Act
        var act = () => Username.Create(" testuser");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*leading or trailing whitespace*");
    }

    [Fact]
    public void Create_WithTrailingWhitespace_ShouldThrow()
    {
        // Act
        var act = () => Username.Create("testuser ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*leading or trailing whitespace*");
    }

    [Fact]
    public void Equals_ShouldBeCaseInsensitive()
    {
        // Arrange
        var username1 = Username.Create("TestUser");
        var username2 = Username.Create("testuser");

        // Assert
        username1.Equals(username2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Act
        string value = username;

        // Assert
        value.Should().Be("testuser");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var username = Username.Create("testuser");

        // Assert
        username.ToString().Should().Be("testuser");
    }

    [Fact]
    public void MinLength_ShouldBe3()
    {
        // Assert
        Username.MinLength.Should().Be(3);
    }

    [Fact]
    public void MaxLength_ShouldBe50()
    {
        // Assert
        Username.MaxLength.Should().Be(50);
    }
}
