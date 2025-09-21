using ParkingSystemAPI.DTOs;

namespace ParkingSystemAPI.Services.Interfaces
{
    public interface IFineService
    {
        Task<IEnumerable<FineDto>> GetAllFinesAsync();
        Task<IEnumerable<FineDto>> GetUserFinesAsync(int userId);
        Task<FineDto> GetFineByIdAsync(int id);
        Task<FineDto> IssueFineAsync(CreateFineDto createFineDto, int issuedByUserId);
        Task<bool> PayFineAsync(int fineId);
        Task<bool> DisputeFineAsync(int fineId, string reason);
        Task<bool> CancelFineAsync(int fineId, int userId);
    }

}
