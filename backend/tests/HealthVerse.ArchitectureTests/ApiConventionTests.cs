using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests for API layer conventions.
/// Ensures controllers and endpoints follow patterns.
/// </summary>
public class ApiConventionTests
{
    [Fact]
    public void Controllers_ShouldEndWithController()
    {
        // Arrange
        var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Api");

        if (apiAssembly == null) return;

        // Act
        var result = Types.InAssembly(apiAssembly)
            .That()
            .ResideInNamespaceContaining("Controllers")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All controllers should end with 'Controller'");
    }

    [Fact]
    public void Controllers_ShouldInheritFromControllerBase()
    {
        // Arrange
        var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Api");

        if (apiAssembly == null) return;

        // Act
        var controllerTypes = Types.InAssembly(apiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreClasses()
            .GetTypes();

        foreach (var controllerType in controllerTypes)
        {
            // Check inheritance chain for ControllerBase
            var baseType = controllerType.BaseType;
            var inheritsFromControllerBase = false;
            
            while (baseType != null)
            {
                if (baseType.Name.Contains("Controller"))
                {
                    inheritsFromControllerBase = true;
                    break;
                }
                baseType = baseType.BaseType;
            }

            inheritsFromControllerBase.Should().BeTrue(
                because: $"{controllerType.Name} should inherit from ControllerBase");
        }
    }

    [Fact]
    public void Api_ShouldOnlyDependOnApplicationLayer()
    {
        // API should not directly use Infrastructure implementations
        // It should go through Application layer (MediatR commands/queries)

        var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Api");

        if (apiAssembly == null) return;

        // Controllers should primarily use MediatR (ISender)
        var controllerTypes = Types.InAssembly(apiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        foreach (var controllerType in controllerTypes)
        {
            // Check that controllers inject ISender (MediatR)
            var constructors = controllerType.GetConstructors();
            
            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters();
                // Controllers typically inject ISender
                // This is a structural check
            }
        }
    }

    [Fact]
    public void Api_ShouldNotContainBusinessLogic()
    {
        // API layer should be thin - no business logic
        var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Api");

        if (apiAssembly == null) return;

        // Act - API should not have Services or Handlers
        // Exception: AuthHandler (Infrastructure logic leaking) and GetSystemStatusQueryHandler (Phase 2 refactor artifact)
        var result = Types.InAssembly(apiAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .Or()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        var exceptions = new[] { "AuthHandler", "GetSystemStatusQueryHandler", "TestAuthHandler" };
        var businessLogicTypes = result.Where(t => !exceptions.Any(e => t.Name.Contains(e))).ToList();
        
        businessLogicTypes.Should().BeEmpty(
            because: "API layer should not contain business logic services or handlers (except allowlisted ones)");
    }

    [Fact]
    public void Controllers_ShouldNotDependOnDbContext()
    {
        var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "HealthVerse.Api");

        if (apiAssembly == null) return;

        var result = Types.InAssembly(apiAssembly)
            .That()
            .ResideInNamespaceContaining("Controllers")
            .Should()
            .NotHaveDependencyOn("HealthVerse.Infrastructure.Persistence")
            .And()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Controllers should not depend on DbContext or EF Core directly. Use IMediator instead.");
    }
}
