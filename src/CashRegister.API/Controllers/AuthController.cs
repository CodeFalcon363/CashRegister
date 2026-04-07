using CashRegister.API.DTOs;
using CashRegister.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.API.Controllers;

// Handles user authentication and token management via JWT.
// Returns access and refresh tokens on successful login.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, ITokenService tokenService, IUserService userService)
    {
        _authService = authService;
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (!result.Success || result.User == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if (!result.User.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            var accessToken = _tokenService.GenerateAccessToken(result.User);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponse(
                result.User.Id,
                result.User.Username,
                result.User.Email,
                result.User.Role.ToString(),
                result.User.BranchId,
                accessToken,
                refreshToken
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message }, ex);
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var userId = _tokenService.ValidateToken(request.RefreshToken);

            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // Fetch user from database to generate proper access token with claims
            var user = await _userService.GetUserByIdAsync(userId.Value);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            // Generate new access token and refresh token
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new TokenResponse(accessToken, refreshToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
