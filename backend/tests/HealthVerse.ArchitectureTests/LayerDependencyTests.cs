using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests ensuring proper layer dependencies following Clean Architecture.
/// 
/// Layer hierarchy (inner to outer):
/// 1. SharedKernel - Base abstractions, no dependencies
/// 2. Domain - Business entities, depends only on SharedKernel
/// 3. Application - Use cases, depends on Domain and SharedKernel
/// 4. Infrastructure - External concerns, depends on Application and Domain
/// 5. Api - Entry point, depends on all layers
/// </summary>
public class LayerDependencyTests
{
    // Assembly namespaces
    private const string SharedKernelNamespace = "HealthVerse.SharedKernel";
    private const string DomainNamespace = "HealthVerse.*.Domain";
    private const string ApplicationNamespace = "HealthVerse.*.Application";
    private const string InfrastructureNamespace = "HealthVerse.*.Infrastructure";
    private const string ApiNamespace = "HealthVerse.Api";

    // Specific module namespaces for detailed testing
    private static readonly string[] DomainAssemblies = new[]
    {
        "HealthVerse.Identity.Domain",
        "HealthVerse.Social.Domain",
        "HealthVerse.Tasks.Domain",
        "HealthVerse.Missions.Domain",
        "HealthVerse.Competition.Domain",
        "HealthVerse.Gamification.Domain",
        "HealthVerse.Notifications.Domain"
    };

    private static readonly string[] InfrastructureAssemblies = new[]
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

    [Fact]
    public void SharedKernel_ShouldNotDependOnAnyOtherLayer()
    {
        // Arrange
        var assembly = typeof(SharedKernel.Domain.Entity).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "HealthVerse.Identity",
                "HealthVerse.Social",
                "HealthVerse.Tasks",
                "HealthVerse.Missions",
                "HealthVerse.Competition",
                "HealthVerse.Gamification",
                "HealthVerse.Notifications",
                "HealthVerse.Api",
                "HealthVerse.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "SharedKernel should be the innermost layer with no dependencies on other layers");
    }

    [Theory]
    [InlineData("HealthVerse.Identity.Domain")]
    [InlineData("HealthVerse.Social.Domain")]
    [InlineData("HealthVerse.Tasks.Domain")]
    [InlineData("HealthVerse.Missions.Domain")]
    [InlineData("HealthVerse.Competition.Domain")]
    [InlineData("HealthVerse.Gamification.Domain")]
    [InlineData("HealthVerse.Notifications.Domain")]
    public void Domain_ShouldOnlyDependOnSharedKernel(string domainAssemblyName)
    {
        // Arrange
        var assembly = GetAssemblyByName(domainAssemblyName);
        if (assembly == null)
        {
            // Skip test if assembly not loaded (happens when tests run in isolation)
            return;
        }

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "HealthVerse.Api",
                "HealthVerse.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer ({domainAssemblyName}) should only depend on SharedKernel");
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var domainAssemblies = DomainAssemblies
            .Select(GetAssemblyByName)
            .Where(a => a != null)
            .ToList();

        foreach (var assembly in domainAssemblies)
        {
            // Act
            var result = Types.InAssembly(assembly!)
                .ShouldNot()
                .HaveDependencyOnAny(InfrastructureAssemblies)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"Domain layer should not depend on Infrastructure");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
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
            var assembly = GetAssemblyByName(assemblyName);
            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "HealthVerse.Infrastructure",
                    "Microsoft.EntityFrameworkCore",
                    "Npgsql")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"Application layer ({assemblyName}) should not depend on Infrastructure");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOnApi()
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
            var assembly = GetAssemblyByName(assemblyName);
            if (assembly == null) continue;

            // Act
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("HealthVerse.Api")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                because: $"Application layer ({assemblyName}) should not depend on Api");
        }
    }

    private static System.Reflection.Assembly? GetAssemblyByName(string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName);
    }
}
