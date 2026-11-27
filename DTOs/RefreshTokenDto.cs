using System.ComponentModel.DataAnnotations;

namespace QLNhanSu.DTOs;

public class RefreshTokenDto
{
    [Required, StringLength(128, MinimumLength = 64)]
    public string RefreshToken { get; set; } = string.Empty;
}

public record TokenPairDto(string Token, string RefreshToken);
