using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.User;
using Online_Learning_Platform_Ass1.Service.Results;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;
using Online_Learning_Platform_Ass1.Service.Utils;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class UserService(
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
            return ServiceResult<Guid>.FailureResultAsync(errors);
        }

        var existingUser = await userRepository.GetByUsernameAsync(dto.Username);
        if (existingUser != null) return ServiceResult<Guid>.FailureResultAsync("Username already exists");

        var existingEmail = await userRepository.GetByEmailAsync(dto.Email);
        if (existingEmail != null) return ServiceResult<Guid>.FailureResultAsync("Email already registered");

        var passwordHash = PasswordUtils.HashPasswordAsync(dto.Password);

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

            return ServiceResult<Guid>.SuccessResultAsync(user.Id, "User registered successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<Guid>.FailureResultAsync($"An error occurred during registration: {ex.Message}");
        }
    }


    public async Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto)
    {
        var validationResult = await loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ServiceResult<UserLoginResponseDto>.FailureResultAsync(errors);
        }

        var user = await userRepository.GetByUsernameAsync(dto.UsernameOrEmail);
        user ??= await userRepository.GetByEmailAsync(dto.UsernameOrEmail);

        if (user is null || !PasswordUtils.VerifyPasswordAsync(dto.Password, user.PasswordHash))
            return ServiceResult<UserLoginResponseDto>.FailureResultAsync("Invalid username/email or password");

        var response = new UserLoginResponseDto(user.Id, user.Username, user.Email, user.CreateAt);

        return ServiceResult<UserLoginResponseDto>.SuccessResultAsync(response, "Login successful");
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.CreateAt));
    }
}
