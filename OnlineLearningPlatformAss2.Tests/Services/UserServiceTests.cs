using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.DTOs.User;
using FluentAssertions;
using Xunit;

namespace OnlineLearningPlatformAss2.Tests.Services;

public class UserServiceTests
{
    private OnlineLearningContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OnlineLearningContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OnlineLearningContext(options);
    }

    private async Task<Role> CreateStudentRole(OnlineLearningContext context)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "Student" };
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        return role;
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("validuser", "valid@email.com", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "validuser");
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_EmptyUsername_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("", "valid@email.com", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Username");
    }

    [Fact]
    public async Task RegisterAsync_WhitespaceUsername_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("   ", "valid@email.com", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("validuser", "", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email");
    }

    [Fact]
    public async Task RegisterAsync_EmptyPassword_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("validuser", "valid@email.com", "", "");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Password");
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("validuser", "valid@email.com", "12345", "12345");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("6 characters");
    }

    [Fact]
    public async Task RegisterAsync_MismatchedPasswords_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        await CreateStudentRole(context);
        var service = new UserService(context);
        var dto = new UserRegisterDto("validuser", "valid@email.com", "password123", "differentpassword");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("match");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var existingUser = new User { Id = Guid.NewGuid(), Username = "existinguser", Email = "existing@email.com", PasswordHash = "hash", RoleId = role.Id };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserRegisterDto("existinguser", "new@email.com", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already taken");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var existingUser = new User { Id = Guid.NewGuid(), Username = "existinguser", Email = "existing@email.com", PasswordHash = "hash", RoleId = role.Id };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserRegisterDto("newuser", "existing@email.com", "password123", "password123");

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already registered");
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = hashedPassword, RoleId = role.Id, IsActive = true, Role = role };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserLoginDto("testuser", "password123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task LoginAsync_ValidEmailCredentials_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = hashedPassword, RoleId = role.Id, IsActive = true, Role = role };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserLoginDto("test@email.com", "password123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_EmptyUsername_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);
        var dto = new UserLoginDto("", "password123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);
        var dto = new UserLoginDto("testuser", "");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Password");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = hashedPassword, RoleId = role.Id, IsActive = true, Role = role };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserLoginDto("testuser", "wrongpassword");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);
        var dto = new UserLoginDto("nonexistent", "password123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ShouldFail()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = hashedPassword, RoleId = role.Id, IsActive = false, Role = role };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserLoginDto("testuser", "password123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("deactivated");
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = role.Id, Role = role };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);

        // Act
        var result = await service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_EmptyGuid_ShouldReturnNull()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);

        // Act
        var result = await service.GetUserByIdAsync(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UserExistsAsync Tests

    [Fact]
    public async Task UserExistsAsync_ExistingUser_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = role.Id };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.UserExistsAsync(user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);

        // Act
        var result = await service.UserExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_ValidData_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = role.Id };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserProfileUpdateDto("New Bio", "/new/avatar.jpg");

        // Act
        var result = await service.UpdateProfileAsync(user.Id, dto);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.Bio.Should().Be("New Bio");
        updatedUser.AvatarUrl.Should().Be("/new/avatar.jpg");
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);
        var dto = new UserProfileUpdateDto("Bio", "/avatar.jpg");

        // Act
        var result = await service.UpdateProfileAsync(Guid.NewGuid(), dto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileAsync_EmptyBio_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = role.Id, Bio = "Old Bio" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);
        var dto = new UserProfileUpdateDto("", "/avatar.jpg");

        // Act
        var result = await service.UpdateProfileAsync(user.Id, dto);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region UpgradeToInstructorAsync Tests

    [Fact]
    public async Task UpgradeToInstructorAsync_ValidUser_ShouldSucceed()
    {
        // Arrange
        using var context = GetDbContext();
        var studentRole = await CreateStudentRole(context);
        var instructorRole = new Role { Id = Guid.NewGuid(), Name = "Instructor" };
        context.Roles.Add(instructorRole);
        await context.SaveChangesAsync();

        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = studentRole.Id };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.UpgradeToInstructorAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.RoleId.Should().Be(instructorRole.Id);
    }

    [Fact]
    public async Task UpgradeToInstructorAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);

        // Act
        var result = await service.UpgradeToInstructorAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpgradeToInstructorAsync_NoInstructorRole_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetDbContext();
        var studentRole = await CreateStudentRole(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@email.com", PasswordHash = "hash", RoleId = studentRole.Id };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.UpgradeToInstructorAsync(user.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WithUsers_ShouldReturnAll()
    {
        // Arrange
        using var context = GetDbContext();
        var role = await CreateStudentRole(context);
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "user1", Email = "u1@test.com", PasswordHash = "hash", RoleId = role.Id, Role = role },
            new User { Id = Guid.NewGuid(), Username = "user2", Email = "u2@test.com", PasswordHash = "hash", RoleId = role.Id, Role = role }
        );
        await context.SaveChangesAsync();

        var service = new UserService(context);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllUsersAsync_EmptyDatabase_ShouldReturnEmpty()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new UserService(context);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
