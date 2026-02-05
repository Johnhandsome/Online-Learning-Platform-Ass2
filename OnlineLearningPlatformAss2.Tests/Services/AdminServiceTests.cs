using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class AdminServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    [Fact]
    public async Task ApproveCourseAsync_ShouldUpdateStatusToPublished()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new AdminService(context);
        var course = new Course { Id = Guid.NewGuid(), Title = "Pending Course", Description = "Test Description", Status = "Pending", InstructorId = Guid.NewGuid() };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ApproveCourseAsync(course.Id);

        // Assert
        result.Should().BeTrue();
        var updatedCourse = await context.Courses.FindAsync(course.Id);
        updatedCourse!.Status.Should().Be("Published");
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldDeactivateActiveUser()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new AdminService(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@test.com", PasswordHash = "hash", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.IsActive.Should().BeFalse();
    }
}
