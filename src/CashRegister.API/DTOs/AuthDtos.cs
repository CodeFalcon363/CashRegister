namespace CashRegister.API.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    int UserId,
    string Username,
    string Email,
    string Role,
    int? BranchId,
    string AccessToken,
    string RefreshToken
);

public record RefreshTokenRequest(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken);
