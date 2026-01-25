using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Service.DTOs.User;
using Online_Learning_Platform_Ass1.Service.Results;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<Guid>> RegisterAsync(UserRegisterDto dto);
    Task<ServiceResult<UserLoginResponseDto>> LoginAsync(UserLoginDto dto);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> HasCompletedAssessmentAsync(Guid userId);
    Task UpdateAssessmentStatusAsync(Guid userId, bool completed);
    Task<User?> GetUserByIdAsync(Guid userId);
}
