using FluentAssertions;
using HealthVerse.SharedKernel.Domain;
using HealthVerse.SharedKernel.ValueObjects;
using Xunit;

namespace HealthVerse.UnitTests.SharedKernel;

public class WeekIdTests
{
    [Theory]
    [InlineData("2025-W01")]
    [InlineData("2025-W52")]
    [InlineData("2024-W53")]
    [InlineData("2025-W10")]
    public void Create_WithValidFormat_ShouldSucceed(string weekIdValue)
    {
        // Act
        var weekId = WeekId.Create(weekIdValue);

        // Assert
        weekId.Value.Should().Be(weekIdValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNull_ShouldThrow(string? weekIdValue)
    {
        // Act
        var act = () => WeekId.Create(weekIdValue!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("2025-01")]
    [InlineData("2025W01")]
    [InlineData("W01-2025")]
    [InlineData("2025-W54")]
    [InlineData("2025-W00")]
    [InlineData("invalid")]
    public void Create_WithInvalidFormat_ShouldThrow(string weekIdValue)
    {
        // Act
        var act = () => WeekId.Create(weekIdValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*must be in format YYYY-Www*");
    }

    [Fact]
    public void FromDate_ShouldCreateCorrectWeekId()
    {
        // Arrange
        var date = new DateOnly(2025, 1, 6); // Monday of Week 2

        // Act
        var weekId = WeekId.FromDate(date);

        // Assert
        weekId.Year.Should().Be(2025);
        weekId.WeekNumber.Should().BeInRange(1, 2); // First or second week depending on calendar
    }

    [Fact]
    public void FromDate_WithDateTimeOffset_ShouldCreateCorrectWeekId()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2025, 1, 6, 12, 0, 0, TimeSpan.FromHours(3));
        var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        // Act
        var weekId = WeekId.FromDate(dateTime, turkeyTimeZone);

        // Assert
        weekId.Year.Should().Be(2025);
    }

    [Fact]
    public void Year_ShouldReturnCorrectValue()
    {
        // Arrange
        var weekId = WeekId.Create("2025-W03");

        // Assert
        weekId.Year.Should().Be(2025);
    }

    [Fact]
    public void WeekNumber_ShouldReturnCorrectValue()
    {
        // Arrange
        var weekId = WeekId.Create("2025-W03");

        // Assert
        weekId.WeekNumber.Should().Be(3);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var weekId = WeekId.Create("2025-W03");

        // Act
        string value = weekId;

        // Assert
        value.Should().Be("2025-W03");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        // Arrange
        var weekId1 = WeekId.Create("2025-W03");
        var weekId2 = WeekId.Create("2025-W03");

        // Assert
        weekId1.Equals(weekId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        // Arrange
        var weekId1 = WeekId.Create("2025-W03");
        var weekId2 = WeekId.Create("2025-W04");

        // Assert
        weekId1.Equals(weekId2).Should().BeFalse();
    }
}
