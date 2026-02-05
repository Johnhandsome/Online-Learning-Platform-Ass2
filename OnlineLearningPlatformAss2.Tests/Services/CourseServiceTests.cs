using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class CourseServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new OnlineLearningContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task EnrollUserAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new CourseService(context, new Mock<IReviewService>().Object);
        var user = new User { Id = Guid.NewGuid(), Username = "student", Email = "student@test.com", PasswordHash = "hash" };
        var instructor = new User { Id = Guid.NewGuid(), Username = "teacher", Email = "teacher@test.com", PasswordHash = "hash" };
        var course = new Course { Id = Guid.NewGuid(), Title = "Test Course", Description = "Test Description", InstructorId = instructor.Id, Price = 100 };
        
        context.Users.AddRange(user, instructor);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EnrollUserAsync(user.Id, course.Id);

        // Assert
        result.Should().BeTrue();
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.UserId == user.Id && e.CourseId == course.Id);
        enrollment.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLessonProgressAsync_ShouldCompleteCourse_WhenAllLessonsFinished()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new CourseService(context, new Mock<IReviewService>().Object);
        var user = new User { Id = Guid.NewGuid(), Username = "student", Email = "student@test.com", PasswordHash = "hash" };
        var instructor = new User { Id = Guid.NewGuid(), Username = "teacher", Email = "teacher@test.com", PasswordHash = "hash" };
        var course = new Course { Id = Guid.NewGuid(), Title = "Test Course", Description = "Test Description", InstructorId = instructor.Id, Price = 100 };
        var module = new Module { Id = Guid.NewGuid(), CourseId = course.Id, Title = "Module 1", Description = "Module Description" };
        var lesson = new Lesson { Id = Guid.NewGuid(), ModuleId = module.Id, Title = "Lesson 1", Content = "Lesson Content" };
        
        context.Users.AddRange(user, instructor);
        context.Courses.Add(course);
        context.Modules.Add(module);
        context.Lessons.Add(lesson);
        
        var enrollment = new Enrollment { Id = Guid.NewGuid(), UserId = user.Id, CourseId = course.Id, Status = "Enrolled" };
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateLessonProgressAsync(enrollment.Id, lesson.Id, true);

        // Assert
        result.Should().BeTrue();
        var updatedEnrollment = await context.Enrollments.FindAsync(enrollment.Id);
        updatedEnrollment!.Status.Should().Be("Completed");
        
        // Verify Certificate issued
        var cert = await context.Certificates.AnyAsync(c => c.UserId == user.Id && c.CourseId == course.Id);
        cert.Should().BeTrue();
    }
}
