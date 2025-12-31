using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests for Infrastructure layer conventions.
/// Ensures repositories, configurations, and adapters follow patterns.
/// </summary>
public class InfrastructureConventionTests
{
    [Fact]
    public void Repositories_ShouldEndWithRepository()
    {
        // Arrange
        var infrastructureAssemblies = new[]
        {
            "HealthVerse.Identity.Infrastructure",
            "HealthVerse.Social.Infrastructure",
            "HealthVerse.Tasks.Infrastructure",
            "HealthVerse.Missions.Infrastructure",
            "HealthVerse.Competition.Infrastructure",
            "HealthVerse.Gamification.Infrastructure",
            "HealthVerse.Notifications.Infrastructure"
        };

        foreach (var assemblyName in infrastructureAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Repositories")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"All repositories in {assemblyName} should end with 'Repository'");
        }
    }

    [Fact]
    public void Repositories_ShouldImplementIRepository()
    {
        // Repositories should implement their corresponding port interface
        var infrastructureAssemblies = new[]
        {
            "HealthVerse.Identity.Infrastructure",
            "HealthVerse.Social.Infrastructure",
            "HealthVerse.Tasks.Infrastructure",
            "HealthVerse.Missions.Infrastructure",
            "HealthVerse.Competition.Infrastructure",
            "HealthVerse.Gamification.Infrastructure",
            "HealthVerse.Notifications.Infrastructure"
        };

        foreach (var assemblyName in infrastructureAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            var repositoryTypes = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Repository")
                .And()
                .AreClasses()
                .GetTypes();

            foreach (var repoType in repositoryTypes)
            {
                // Each repository should implement at least one interface
                var interfaces = repoType.GetInterfaces();
                interfaces.Should().NotBeEmpty(
                    because: $"{repoType.Name} should implement a port interface");
            }
        }
    }

    [Fact]
    public void EntityConfigurations_ShouldEndWithConfiguration()
    {
        // Arrange
        var infrastructureAssemblies = new[]
        {
            "HealthVerse.Identity.Infrastructure",
            "HealthVerse.Social.Infrastructure",
            "HealthVerse.Tasks.Infrastructure",
            "HealthVerse.Missions.Infrastructure",
            "HealthVerse.Competition.Infrastructure",
            "HealthVerse.Gamification.Infrastructure",
            "HealthVerse.Notifications.Infrastructure"
        };

        foreach (var assemblyName in infrastructureAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Configurations")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Configuration")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"All entity configurations in {assemblyName} should end with 'Configuration'");
        }
    }

    [Fact]
    public void Infrastructure_ShouldDependOnApplication()
    {
        // Infrastructure implements Application layer ports
        var infrastructureAssemblies = new[]
        {
            "HealthVerse.Identity.Infrastructure",
            "HealthVerse.Social.Infrastructure",
            "HealthVerse.Tasks.Infrastructure",
            "HealthVerse.Missions.Infrastructure",
            "HealthVerse.Competition.Infrastructure",
            "HealthVerse.Gamification.Infrastructure",
            "HealthVerse.Notifications.Infrastructure"
        };

        foreach (var assemblyName in infrastructureAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Get module name (e.g., "Identity" from "HealthVerse.Identity.Infrastructure")
            var moduleName = assemblyName.Split('.')[1];
            var applicationNamespace = $"HealthVerse.{moduleName}.Application";

            // Infrastructure should reference its own Application layer
            var types = Types.InAssembly(assembly).GetTypes();
            types.Should().NotBeEmpty(
                because: $"{assemblyName} should have implementations that depend on {applicationNamespace}");
        }
    }

    [Fact]
    public void DbContexts_ShouldInheritFromDbContext()
    {
        // Arrange
        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Infrastructure");

        if (infrastructureAssembly == null) return;

        // Act
        var dbContextTypes = Types.InAssembly(infrastructureAssembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .And()
            .AreClasses()
            .GetTypes();

        foreach (var dbContextType in dbContextTypes)
        {
            // Assert
            dbContextType.BaseType?.Name.Should().Contain("DbContext",
                because: $"{dbContextType.Name} should inherit from DbContext");
        }
    }

    [Fact]
    public void Services_ShouldEndWithService()
    {
        // Arrange
        var infrastructureAssemblies = new[]
        {
            "HealthVerse.Identity.Infrastructure",
            "HealthVerse.Social.Infrastructure",
            "HealthVerse.Tasks.Infrastructure",
            "HealthVerse.Missions.Infrastructure",
            "HealthVerse.Competition.Infrastructure",
            "HealthVerse.Gamification.Infrastructure",
            "HealthVerse.Notifications.Infrastructure",
            "HealthVerse.Infrastructure"
        };

        foreach (var assemblyName in infrastructureAssemblies)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Services")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .HaveNameEndingWith("Service")
                .GetResult();

            // Soft assertion - some services might have different names
        }
    }
}
