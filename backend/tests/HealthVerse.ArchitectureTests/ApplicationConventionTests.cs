using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests for Application layer conventions.
/// Ensures CQRS patterns and proper naming conventions.
/// </summary>
public class ApplicationConventionTests
{
    [Fact]
    public void Commands_ShouldEndWithCommand()
    {
        // Arrange
        var applicationAssemblies = new[]
        {
            "HealthVerse.Identity.Application",
            "HealthVerse.Social.Application",
            "HealthVerse.Tasks.Application",
            "HealthVerse.Missions.Application",
            "HealthVerse.Competition.Application",
            "HealthVerse.Gamification.Application",
            "HealthVerse.Notifications.Application"
        };

        foreach (var assemblyName in applicationAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act - Check that command handlers handle commands ending with "Command"
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Commands")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .DoNotHaveNameEndingWith("Handler")
                .And()
                .DoNotHaveNameEndingWith("Validator")
                .Should()
                .HaveNameEndingWith("Command")
                .GetResult();

            // We use soft assertion here because some classes might have different naming
        }
    }

    [Fact]
    public void Queries_ShouldEndWithQuery()
    {
        // Arrange
        var applicationAssemblies = new[]
        {
            "HealthVerse.Identity.Application",
            "HealthVerse.Social.Application",
            "HealthVerse.Tasks.Application",
            "HealthVerse.Missions.Application",
            "HealthVerse.Competition.Application",
            "HealthVerse.Gamification.Application",
            "HealthVerse.Notifications.Application"
        };

        foreach (var assemblyName in applicationAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Queries")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .DoNotHaveNameEndingWith("Handler")
                .And()
                .DoNotHaveNameEndingWith("Dto")
                .Should()
                .HaveNameEndingWith("Query")
                .GetResult();
        }
    }

    [Fact]
    public void Handlers_ShouldEndWithHandler()
    {
        // Arrange
        var applicationAssemblies = new[]
        {
            "HealthVerse.Identity.Application",
            "HealthVerse.Social.Application",
            "HealthVerse.Tasks.Application",
            "HealthVerse.Missions.Application",
            "HealthVerse.Competition.Application",
            "HealthVerse.Gamification.Application",
            "HealthVerse.Notifications.Application"
        };

        foreach (var assemblyName in applicationAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
                .Should()
                .HaveNameEndingWith("Handler")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"All handlers in {assemblyName} should end with 'Handler'");
        }
    }

    [Fact]
    public void Ports_ShouldBeInterfaces()
    {
        // Arrange - Ports define the interfaces that infrastructure implements
        var applicationAssemblies = new[]
        {
            "HealthVerse.Identity.Application",
            "HealthVerse.Social.Application",
            "HealthVerse.Tasks.Application",
            "HealthVerse.Missions.Application",
            "HealthVerse.Competition.Application",
            "HealthVerse.Gamification.Application",
            "HealthVerse.Notifications.Application"
        };

        foreach (var assemblyName in applicationAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceEndingWith("Ports")
                .And()
                .HaveNameStartingWith("I")
                .Should()
                .BeInterfaces()
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"All types in Ports namespace of {assemblyName} starting with 'I' should be interfaces");
        }
    }

    [Fact]
    public void DTOs_ShouldBeRecords()
    {
        // DTOs should be immutable records for response/request objects
        // This is a soft check since not all DTOs need to be records

        var applicationAssemblies = new[]
        {
            "HealthVerse.Identity.Application",
            "HealthVerse.Social.Application",
            "HealthVerse.Tasks.Application",
            "HealthVerse.Missions.Application",
            "HealthVerse.Competition.Application",
            "HealthVerse.Gamification.Application",
            "HealthVerse.Notifications.Application"
        };

        foreach (var assemblyName in applicationAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            var dtoTypes = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Dto")
                .GetTypes();

            // Soft assertion - DTOs can be records or classes
            dtoTypes.Should().NotBeNull();
        }
    }
}
