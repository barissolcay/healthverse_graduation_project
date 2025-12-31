using FluentAssertions;
using HealthVerse.Identity.Domain.Entities;
using HealthVerse.SharedKernel.Domain;
using Xunit;

namespace HealthVerse.UnitTests.Identity;

public class AuthIdentityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAuthIdentity()
    {
        // Arrange
        var firebaseUid = "firebase-uid-123";
        var userId = Guid.NewGuid();
        var provider = "GOOGLE";
        var providerEmail = "user@gmail.com";

        // Act
        var authIdentity = AuthIdentity.Create(firebaseUid, userId, provider, providerEmail);

        // Assert
        authIdentity.Id.Should().NotBe(Guid.Empty);
        authIdentity.FirebaseUid.Should().Be(firebaseUid);
        authIdentity.UserId.Should().Be(userId);
        authIdentity.Provider.Should().Be("GOOGLE");
        authIdentity.ProviderEmail.Should().Be(providerEmail);
        authIdentity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        authIdentity.LastLoginAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldUppercaseProvider()
    {
        // Arrange
        var firebaseUid = "firebase-uid-123";
        var userId = Guid.NewGuid();
        var provider = "google";

        // Act
        var authIdentity = AuthIdentity.Create(firebaseUid, userId, provider);

        // Assert
        authIdentity.Provider.Should().Be("GOOGLE");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFirebaseUid_ShouldThrow(string? firebaseUid)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => AuthIdentity.Create(firebaseUid!, userId, "GOOGLE");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        // Act
        var act = () => AuthIdentity.Create("firebase-uid", Guid.Empty, "GOOGLE");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*UserId cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyProvider_ShouldThrow(string? provider)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => AuthIdentity.Create("firebase-uid", userId, provider!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Provider cannot be empty*");
    }

    [Fact]
    public void UpdateLastLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var authIdentity = AuthIdentity.Create("firebase-uid", Guid.NewGuid(), "GOOGLE");
        var originalLastLogin = authIdentity.LastLoginAt;
        
        // Wait a tiny bit to ensure time difference
        Thread.Sleep(10);

        // Act
        authIdentity.UpdateLastLogin();

        // Assert
        authIdentity.LastLoginAt.Should().BeAfter(originalLastLogin);
    }

    [Fact]
    public void AuthProvider_Constants_ShouldBeCorrect()
    {
        // Assert
        AuthProvider.GOOGLE.Should().Be("GOOGLE");
        AuthProvider.APPLE.Should().Be("APPLE");
        AuthProvider.EMAIL.Should().Be("EMAIL");
    }
}
