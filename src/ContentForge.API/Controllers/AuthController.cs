using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ContentForge.API.Controllers;

// [ApiController] = tells ASP.NET this is a REST controller (auto model binding, validation, etc.)
// [Route("auth")] = base path. Like router = express.Router(); app.use('/auth', router)
// `: ControllerBase` = base class providing helper methods (Ok(), NotFound(), Unauthorized(), etc.)
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    // IConfiguration = reads from appsettings.json + env vars. Like process.env + config files.
    // Access values with ["Section:Key"] syntax (colon-separated path).
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    // Constructor injection — ASP.NET auto-provides these from the DI container.
    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // [AllowAnonymous] = skip JWT auth for this endpoint (otherwise [Authorize] would block it).
    // [HttpPost("token")] = POST /auth/token. Like router.post('/token', handler).
    // [FromBody] = parse JSON request body into the TokenRequest record. Like express.json() middleware.
    // IActionResult = the return type for HTTP responses (can be 200, 401, 404, etc.)
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

    // Returns a tuple (string, DateTime) — like returning { token, expiresAt } from a JS function.
    // Tuple syntax: (Type1 Name1, Type2 Name2) — a lightweight way to return multiple values.
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
