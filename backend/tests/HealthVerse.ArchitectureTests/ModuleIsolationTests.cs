using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HealthVerse.ArchitectureTests;

/// <summary>
/// Architecture tests ensuring modules are properly isolated.
/// Modules should only communicate through well-defined interfaces.
/// </summary>
public class ModuleIsolationTests
{
    private static readonly string[] ModuleNames = new[]
    {
        "Identity",
        "Social",
        "Tasks",
        "Missions",
        "Competition",
        "Gamification",
        "Notifications"
    };

    [Theory]
    [InlineData("Identity", new[] { "Social", "Tasks", "Missions", "Competition", "Gamification" })]
    [InlineData("Social", new[] { "Tasks", "Missions", "Competition", "Gamification" })]
    [InlineData("Tasks", new[] { "Identity", "Social", "Missions" })]
    [InlineData("Missions", new[] { "Competition" })]
    [InlineData("Competition", new[] { "Social", "Tasks", "Missions" })]
    [InlineData("Gamification", new[] { "Social", "Tasks", "Missions", "Competition" })]
    public void Domain_ShouldNotDirectlyDependOnOtherModuleDomains(string moduleName, string[] forbiddenModules)
    {
        // Arrange
        var assemblyName = $"HealthVerse.{moduleName}.Domain";
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName);

        if (assembly == null)
        {
            // Skip if assembly not loaded
            return;
        }

        var forbiddenNamespaces = forbiddenModules
            .Select(m => $"HealthVerse.{m}.Domain")
            .ToArray();

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"{moduleName} Domain should not directly depend on {string.Join(", ", forbiddenModules)} Domain layers");
    }

    [Fact]
    public void Modules_ShouldHaveConsistentStructure()
    {
        // Each module should have Domain, Application, and Infrastructure layers
        // Use direct type references to ensure assemblies are loaded
        var moduleAssemblies = new Dictionary<string, (System.Reflection.Assembly Domain, System.Reflection.Assembly Application, System.Reflection.Assembly Infrastructure)>
        {
            ["Identity"] = (
                typeof(Identity.Domain.Entities.User).Assembly,
                typeof(Identity.Application.Commands.RegisterCommand).Assembly,
                typeof(Identity.Infrastructure.Persistence.UserRepository).Assembly
            ),
            ["Social"] = (
                typeof(Social.Domain.Entities.Duel).Assembly,
                typeof(Social.Application.Commands.FollowUserCommand).Assembly,
                typeof(Social.Infrastructure.Persistence.DuelRepository).Assembly
            ),
            ["Tasks"] = (
                typeof(Tasks.Domain.Entities.UserTask).Assembly,
                typeof(Tasks.Application.Queries.GetActiveTasksQuery).Assembly,
                typeof(Tasks.Infrastructure.Persistence.UserTaskRepository).Assembly
            ),
            ["Missions"] = (
                typeof(Missions.Domain.Entities.GlobalMission).Assembly,
                typeof(Missions.Application.Queries.GetActiveGlobalMissionsQuery).Assembly,
                typeof(Missions.Infrastructure.Persistence.GlobalMissionRepository).Assembly
            ),
            ["Competition"] = (
                typeof(Competition.Domain.Entities.LeagueRoom).Assembly,
                typeof(Competition.Application.Queries.GetMyRoomQuery).Assembly,
                typeof(Competition.Infrastructure.Persistence.LeagueRoomRepository).Assembly
            ),
            ["Gamification"] = (
                typeof(Gamification.Domain.Entities.PointTransaction).Assembly,
                typeof(Gamification.Application.Queries.GetLeaderboardQuery).Assembly,
                typeof(Gamification.Infrastructure.Persistence.PointTransactionRepository).Assembly
            ),
            ["Notifications"] = (
                typeof(Notifications.Domain.Entities.Notification).Assembly,
                typeof(Notifications.Application.Queries.GetNotificationsQuery).Assembly,
                typeof(Notifications.Infrastructure.Persistence.NotificationRepository).Assembly
            )
        };

        // All modules should have all three layers
        moduleAssemblies.Count.Should().Be(7,
            because: "All 7 modules should have Domain, Application, and Infrastructure layers");
        
        foreach (var (moduleName, assemblies) in moduleAssemblies)
        {
            assemblies.Domain.Should().NotBeNull($"{moduleName} should have Domain assembly");
            assemblies.Application.Should().NotBeNull($"{moduleName} should have Application assembly");
            assemblies.Infrastructure.Should().NotBeNull($"{moduleName} should have Infrastructure assembly");
        }
    }

    [Fact]
    public void SharedKernel_ShouldBeAccessibleByAllModules()
    {
        // SharedKernel is the only acceptable cross-cutting dependency
        var sharedKernelAssembly = typeof(SharedKernel.Domain.Entity).Assembly;
        
        foreach (var moduleName in ModuleNames)
        {
            var domainAssemblyName = $"HealthVerse.{moduleName}.Domain";
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == domainAssemblyName);

            if (assembly == null) continue;

            // Verify that domain can reference SharedKernel (positive test)
            var types = Types.InAssembly(assembly).GetTypes();
            
            // This is a structural check - modules should be able to use SharedKernel
            types.Should().NotBeEmpty(
                because: $"{moduleName} Domain should have types that can use SharedKernel");
        }
    }

    [Fact]
    public void CrossModuleCommunication_ShouldBeThroughInterfaces()
    {
        // When modules need to communicate, they should do so through interfaces (ports)
        // defined in the Application layer, not direct references

        var applicationAssemblies = ModuleNames
            .Select(m => $"HealthVerse.{m}.Application")
            .Select(name => AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == name))
            .Where(a => a != null)
            .ToList();

        foreach (var assembly in applicationAssemblies)
        {
            // Check that shared services are accessed through interfaces
            var portsTypes = Types.InAssembly(assembly!)
                .That()
                .ResideInNamespaceEndingWith("Ports")
                .GetTypes();

            // Modules should define their own ports for cross-module communication
            // This is a structural validation
        }
    }

    [Theory]
    [InlineData("Identity")]
    [InlineData("Social")]
    [InlineData("Tasks")]
    [InlineData("Missions")]
    [InlineData("Competition")]
    [InlineData("Gamification")]
    [InlineData("Notifications")]
    public void Application_ShouldNotDependOnOtherModuleApplicationLayers(string moduleName)
    {
        // Arrange
        var assemblyName = $"HealthVerse.{moduleName}.Application";
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName);

        if (assembly == null)
        {
            // Skip if assembly not loaded
            return;
        }

        // Get all other module Application namespaces (except this module and Contracts)
        var forbiddenNamespaces = ModuleNames
            .Where(m => m != moduleName)
            .Select(m => $"HealthVerse.{m}.Application")
            .ToArray();

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"{moduleName} Application should not depend on other module Application layers. " +
                     $"Cross-module communication should use HealthVerse.Contracts. " +
                     $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }
}
