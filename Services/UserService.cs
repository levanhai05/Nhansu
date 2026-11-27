using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QLNhanSu.DTOs;
using QLNhanSu.Exceptions;
using Microsoft.Data.SqlClient;
using QLNhanSu.DTOs.Common;
using QLNhanSu.Models;
using QLNhanSu.Repositories;
using QLNhanSu.Settings;

namespace QLNhanSu.Services
{
    public class UserService : IUserService
    {
        private static readonly string[] AllowedRoles = { "User", "HR", "Admin" };

        private readonly IUserRepository _userRepo;
        private readonly JwtSettings _jwt;

        public UserService(IUserRepository userRepo, IOptions<JwtSettings> jwtOptions)
        {
            _userRepo = userRepo;
            _jwt = jwtOptions.Value;
        }

        public async Task<PagedResult<UserResponseDto>> GetAllAsync(UserQueryParameters queryParams)
        {
            var pagedUsers = await _userRepo.GetAllAsync(queryParams);

            var mappedItems = pagedUsers.Items.Select(MapToResponseDto).ToList();

            return new PagedResult<UserResponseDto>(
                mappedItems,
                pagedUsers.TotalCount,
                pagedUsers.PageNumber,
                pagedUsers.PageSize
            );
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            return user == null ? null : MapToResponseDto(user);
        }

        public async Task<(bool IsSuccess, string Message, UserResponseDto? Data)> RegisterAsync(UserRegisterDto request)
        {
            var existingUser = await _userRepo.GetByUsernameOrEmailAsync(request.Username, request.Email);
            if (existingUser != null)
            {
                return (false, "Username hoặc Email đã được sử dụng.", null);
            }

            // Role bỏ trống -> "User"; có truyền thì chỉ chấp nhận các role hợp lệ
            var role = NormalizeRole(request.Role, defaultRole: "User");
            if (role == null)
            {
                return (false, "Role không hợp lệ. Chỉ chấp nhận: User, HR, Admin.", null);
            }

            PasswordHashing.Create(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(newUser);
            await _userRepo.SaveChangesAsync();

            return (true, "Đăng ký thành công", MapToResponseDto(newUser));
        }

        public async Task<(bool IsSuccess, string Message, TokenPairDto? Tokens)> LoginAsync(LoginDto request)
        {
            var user = await _userRepo.GetByUsernameOrEmailAsync(request.Username, request.Username);
            if (user == null || !PasswordHashing.Verify(request.Password, user.PasswordHash, user.PasswordSalt, out var needsUpgrade))
            {
                return (false, "Sai tên đăng nhập hoặc mật khẩu.", null);
            }

            if (needsUpgrade)
            {
                PasswordHashing.Create(request.Password, out var upgradedHash, out var upgradedSalt);
                user.PasswordHash = upgradedHash;
                user.PasswordSalt = upgradedSalt;
            }
            var tokens = RotateRefreshToken(user);
            try { await _userRepo.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            { return (false, "Tài khoản vừa thay đổi, vui lòng đăng nhập lại.", null); }
            return (true, "Đăng nhập thành công", tokens);
        }

        private static string HashRefreshToken(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private TokenPairDto RotateRefreshToken(User user)
        {
            var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.RefreshTokenHash = HashRefreshToken(raw);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            return new TokenPairDto(CreateToken(user), raw);
        }

        public async Task<TokenPairDto?> RefreshAsync(string refreshToken)
        {
            var user = await _userRepo.GetByRefreshTokenHashAsync(HashRefreshToken(refreshToken));
            if (user == null || user.RefreshTokenExpiresAt == null ||
                user.RefreshTokenExpiresAt <= DateTime.UtcNow) return null;
            var tokens = RotateRefreshToken(user);
            try { await _userRepo.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return null; }
            return tokens;
        }

        public async Task<bool> RevokeAsync(string refreshToken)
        {
            var user = await _userRepo.GetByRefreshTokenHashAsync(HashRefreshToken(refreshToken));
            if (user == null) return false;
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            try { await _userRepo.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return false; }
            return true;
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(int id, UserUpdateDto request)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return (false, $"Không tìm thấy người dùng có Id = {id}");
            }

            if ((request.Username != null && string.IsNullOrWhiteSpace(request.Username)) ||
                (request.Email != null && string.IsNullOrWhiteSpace(request.Email)) ||
                (request.Role != null && string.IsNullOrWhiteSpace(request.Role)) ||
                (request.Password != null && string.IsNullOrWhiteSpace(request.Password)))
                throw new BadRequestException("Thông tin cập nhật không được chỉ chứa khoảng trắng.");

            if (request.Username == null && request.Email == null &&
                request.Role == null && request.Password == null)
                return (true, "Không có thông tin cần cập nhật.");

            var username = string.IsNullOrWhiteSpace(request.Username)
                ? user.Username : request.Username.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email)
                ? user.Email : request.Email.Trim();

            if (await _userRepo.ExistsForOtherUserAsync(id, username, email))
                throw new AppException("Username hoặc Email đã được sử dụng.", 409);

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var newRole = NormalizeRole(request.Role, defaultRole: "User");
                if (newRole == null)
                {
                    throw new BadRequestException("Role không hợp lệ. Chỉ chấp nhận: User, HR, Admin.");
                }
                user.Role = newRole;
            }

            user.Email = email;
            user.Username = username;

            if (!string.IsNullOrEmpty(request.Password))
            {
                PasswordHashing.Create(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            // Mọi thay đổi thông tin tài khoản đều thu hồi refresh token hiện tại.
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            _userRepo.Update(user);
            try
            {
                await _userRepo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("Tài khoản vừa được cập nhật. Vui lòng thử lại.", 409);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                sql.Number is 2601 or 2627)
            {
                throw new AppException("Dữ liệu tài khoản bị trùng.", 409);
            }

            return (true, $"Cập nhật thành công người dùng Id = {id}");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return (false, $"Không tìm thấy người dùng có Id = {id}");
            }

            _userRepo.Delete(user);
            await _userRepo.SaveChangesAsync();

            return (true, $"Đã xóa người dùng Id = {id}");
        }

        // Helper Private Methods

        // Trả về role chuẩn hoá (đúng hoa/thường), hoặc null nếu role không hợp lệ.
        private static string? NormalizeRole(string? role, string defaultRole)
        {
            if (string.IsNullOrWhiteSpace(role)) return defaultRole;

            return AllowedRoles.FirstOrDefault(r =>
                r.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static UserResponseDto MapToResponseDto(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        private string CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}