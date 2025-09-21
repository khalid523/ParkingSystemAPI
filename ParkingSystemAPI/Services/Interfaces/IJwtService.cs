using ParkingSystemAPI.Models;
using System.Security.Claims;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal ValidateToken(string token);
        int GetUserIdFromToken(string token);
    }
}
