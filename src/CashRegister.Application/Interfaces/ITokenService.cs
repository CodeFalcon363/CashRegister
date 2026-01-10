using CashRegister.Domain.Entities;

namespace CashRegister.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int? ValidateToken(string token);
}
