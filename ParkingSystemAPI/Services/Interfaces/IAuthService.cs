using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }

}
