using QLNhanSu.DTOs;
using QLNhanSu.DTOs.Common;
namespace QLNhanSu.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserResponseDto>> GetAllAsync(UserQueryParameters queryParams);
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<(bool IsSuccess, string Message, UserResponseDto? Data)> RegisterAsync(UserRegisterDto request);
        Task<(bool IsSuccess, string Message)> UpdateAsync(int id, UserUpdateDto request);
        Task<(bool IsSuccess, string Message)> DeleteAsync(int id);
        Task<(bool IsSuccess, string Message, TokenPairDto? Tokens)> LoginAsync(LoginDto request);
        Task<TokenPairDto?> RefreshAsync(string refreshToken);
        Task<bool> RevokeAsync(string refreshToken);
    }
}