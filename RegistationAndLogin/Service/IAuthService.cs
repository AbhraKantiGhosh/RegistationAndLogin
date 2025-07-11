using RegistationAndLogin.Domain.DTOs;

namespace RegistationAndLogin.Service
{
    public interface IAuthService
    {
        Task<AuthResponceDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponceDto> LoginAsync(LogInDto logInDto);
        Task<UserDto?> GetUserDetailsAsync(Guid userId);
        Task<UserDto> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto);
    }
}
