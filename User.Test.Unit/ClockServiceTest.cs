using System;
using FluentAssertions;
using User.Services;
using Xunit;

namespace User.Test.Unit;

public class ClockServiceTest
{
    [Fact]
    public void UtcNow_ShouldReturnUtcNow()
    {
        // Arrange
        var expectedUtcNow = DateTime.UtcNow;
        var service = new ClockService();
        
        // Act
        var now = service.NowUtc();
        
        // Assert
        now.Kind.Should().Be(DateTimeKind.Utc);
        now.Should().BeCloseTo(expectedUtcNow, TimeSpan.FromSeconds(5));
    }
}