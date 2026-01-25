using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.User;
using Online_Learning_Platform_Ass1.Service.Results;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;
using Online_Learning_Platform_Ass1.Service.Utils;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class UserService
(
    IUserRepository userRepository,
    IValidator<UserRegisterDto> registerValidator,
    IValidator<UserLoginDto> loginValidator
) : IUserService
{
    public async Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto)
    {
        var validationResult = await registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ServiceResult<Guid>.FailureResult(errors);
        }

        if (await userRepository.GetByUsernameAsync(dto.Username) is not null)
            return ServiceResult<Guid>.FailureResult("Username already exists");

        if (await userRepository.GetByEmailAsync(dto.Email) is not null)
            return ServiceResult<Guid>.FailureResult("Email already registered");

        var passwordHash = PasswordUtils.HashPassword(dto.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            CreateAt = DateTime.UtcNow
        };

        try
        {
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();

            return ServiceResult<Guid>.SuccessResult(user.Id, "User registered successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<Guid>.FailureResult($"An error occurred: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto)
    {
        var validationResult = await loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ServiceResult<UserLoginResponseDto>.FailureResult(
                [.. validationResult.Errors.Select(e => e.ErrorMessage)]
            );
        }

        var user = await userRepository.GetByUsernameAsync(dto.UsernameOrEmail)
                   ?? await userRepository.GetByEmailAsync(dto.UsernameOrEmail);

        if (user is null || !PasswordUtils.VerifyPassword(dto.Password, user.PasswordHash))
            return ServiceResult<UserLoginResponseDto>.FailureResult("Invalid username/email or password");

        var response = new UserLoginResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role?.Name ?? "User",
            user.CreateAt
        );

        return ServiceResult<UserLoginResponseDto>.SuccessResult(response, "Login successful");
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role?.Name ?? "Unassigned", u.CreateAt));
    }

    public async Task<bool> HasCompletedAssessmentAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user?.HasCompletedAssessment ?? false;
    }

    public async Task UpdateAssessmentStatusAsync(Guid userId, bool completed)
    {
        var user = await userRepository.GetByIdForUpdateAsync(userId);
        if (user != null)
        {
            user.HasCompletedAssessment = completed;
            user.AssessmentCompletedAt = completed ? DateTime.UtcNow : null;
            await userRepository.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await userRepository.GetByIdAsync(userId);
    }
}
