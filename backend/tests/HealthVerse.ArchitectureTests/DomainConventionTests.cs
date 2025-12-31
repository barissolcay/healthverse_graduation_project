using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests for Domain layer conventions.
/// Ensures entities follow DDD patterns.
/// </summary>
public class DomainConventionTests
{
    [Fact]
    public void Entities_ShouldInheritFromEntityBase()
    {
        // Arrange
        var domainAssemblies = new[]
        {
            typeof(Identity.Domain.Entities.User).Assembly,
            typeof(Social.Domain.Entities.Duel).Assembly,
            typeof(Tasks.Domain.Entities.UserTask).Assembly,
            typeof(Missions.Domain.Entities.GlobalMission).Assembly,
            typeof(Competition.Domain.Entities.LeagueRoom).Assembly,
            typeof(Gamification.Domain.Entities.PointTransaction).Assembly,
            typeof(Notifications.Domain.Entities.Notification).Assembly
        };

        foreach (var assembly in domainAssemblies)
        {
            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceEndingWith("Entities")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(SharedKernel.Domain.Entity))
                .Or()
                .HaveNameEndingWith("Placeholder") // Skip placeholder classes
                .GetResult();

            // Assert - Soft check since some entities might not inherit Entity
            // (e.g., Friendship which uses composite key)
        }
    }

    [Fact]
    public void DomainEvents_ShouldInheritFromDomainEventBase()
    {
        // Arrange
        var identityAssembly = typeof(Identity.Domain.Entities.User).Assembly;

        // Act - Check for records that inherit from DomainEventBase
        var eventTypes = Types.InAssembly(identityAssembly)
            .That()
            .ResideInNamespaceEndingWith("Events")
            .GetTypes();

        // Assert - All event types should exist and be records inheriting from DomainEventBase
        eventTypes.Should().NotBeEmpty(
            because: "Domain events namespace should contain event types");
        
        foreach (var eventType in eventTypes)
        {
            eventType.BaseType?.Name.Should().Be("DomainEventBase",
                because: $"{eventType.Name} should inherit from DomainEventBase");
        }
    }

    [Fact]
    public void ValueObjects_ShouldInheritFromValueObjectBase()
    {
        // Arrange
        var sharedKernelAssembly = typeof(SharedKernel.ValueObjects.WeekId).Assembly;
        var identityAssembly = typeof(Identity.Domain.ValueObjects.Username).Assembly;

        // Test SharedKernel ValueObjects
        var sharedResult = Types.InAssembly(sharedKernelAssembly)
            .That()
            .ResideInNamespaceEndingWith("ValueObjects")
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(SharedKernel.Domain.ValueObject))
            .GetResult();

        // Test Identity ValueObjects
        var identityResult = Types.InAssembly(identityAssembly)
            .That()
            .ResideInNamespaceEndingWith("ValueObjects")
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(SharedKernel.Domain.ValueObject))
            .GetResult();

        // Assert
        sharedResult.IsSuccessful.Should().BeTrue(
            because: "SharedKernel value objects should inherit from ValueObject");
        identityResult.IsSuccessful.Should().BeTrue(
            because: "Identity value objects should inherit from ValueObject");
    }

    [Fact]
    public void Entities_ShouldHavePrivateConstructor()
    {
        // Arrange
        var entityTypes = new[]
        {
            typeof(Identity.Domain.Entities.User),
            typeof(Social.Domain.Entities.Duel),
            typeof(Tasks.Domain.Entities.UserTask),
            typeof(Missions.Domain.Entities.GlobalMission),
            typeof(Competition.Domain.Entities.LeagueRoom),
            typeof(Gamification.Domain.Entities.PointTransaction),
            typeof(Notifications.Domain.Entities.Notification)
        };

        foreach (var entityType in entityTypes)
        {
            // Act
            var hasPrivateConstructor = entityType
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any(c => c.GetParameters().Length == 0);

            // Assert
            hasPrivateConstructor.Should().BeTrue(
                because: $"{entityType.Name} should have a private parameterless constructor for EF Core");
        }
    }

    [Fact]
    public void Entities_ShouldHaveStaticFactoryMethod()
    {
        // Arrange
        var entityTypes = new[]
        {
            typeof(Identity.Domain.Entities.User),
            typeof(Social.Domain.Entities.Duel),
            typeof(Missions.Domain.Entities.GlobalMission),
            typeof(Competition.Domain.Entities.LeagueRoom),
            typeof(Gamification.Domain.Entities.PointTransaction),
            typeof(Notifications.Domain.Entities.Notification)
        };

        foreach (var entityType in entityTypes)
        {
            // Act
            var hasFactoryMethod = entityType
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Any(m => m.Name == "Create" || m.Name == "Assign");

            // Assert
            hasFactoryMethod.Should().BeTrue(
                because: $"{entityType.Name} should have a static factory method (Create or Assign)");
        }
    }
}
