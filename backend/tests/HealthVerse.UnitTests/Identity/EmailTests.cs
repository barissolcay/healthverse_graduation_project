using FluentAssertions;
using HealthVerse.Identity.Domain.ValueObjects;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Identity;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.org")]
    [InlineData("user123@test.co")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNull_ShouldThrow(string? email)
    {
        // Act
        var act = () => Email.Create(email!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user domain.com")]
    public void Create_WithInvalidFormat_ShouldThrow(string email)
    {
        // Act
        var act = () => Email.Create(email);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*format is invalid*");
    }

    [Fact]
    public void Create_WithTooLong_ShouldThrow()
    {
        // Arrange - Generate email with 101+ characters
        // 92 x's + @test.com (9 chars) = 101 characters
        var email = new string('x', 92) + "@test.com";

        // Act
        var act = () => Email.Create(email);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 100 characters*");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Arrange
        var email = "User@Example.COM";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var email = "  user@example.com  ";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldBeTrue()
    {
        // Arrange
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("USER@EXAMPLE.COM");

        // Assert
        email1.Equals(email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldBeFalse()
    {
        // Arrange
        var email1 = Email.Create("user1@example.com");
        var email2 = Email.Create("user2@example.com");

        // Assert
        email1.Equals(email2).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("user@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("user@example.com");
    }

    [Fact]
    public void MaxLength_ShouldBe100()
    {
        // Assert
        Email.MaxLength.Should().Be(100);
    }
}
