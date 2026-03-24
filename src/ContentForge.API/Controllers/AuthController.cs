using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ContentForge.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Exchange your API key for a JWT token.
    /// POST /auth/token with { "apiKey": "your-key" }
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] TokenRequest request)
    {
        var configuredKey = _configuration["Auth:ApiKey"];
        if (string.IsNullOrEmpty(configuredKey) || request.ApiKey != configuredKey)
        {
            _logger.LogWarning("Invalid token request from {IP}", HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = "Invalid API key." });
        }

        var token = GenerateJwtToken();
        _logger.LogInformation("JWT token issued successfully");

        return Ok(new TokenResponse(token.Token, token.ExpiresAt));
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken()
    {
        var secret = _configuration["Auth:JwtSecret"]
            ?? throw new InvalidOperationException("JWT secret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("Auth:TokenExpirationHours", 24));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ContentForge-Operator"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Auth:Issuer"] ?? "ContentForge",
            audience: _configuration["Auth:Audience"] ?? "ContentForge-API",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public record TokenRequest(string ApiKey);
public record TokenResponse(string Token, DateTime ExpiresAt);
