using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

[ApiController]
[Route("api/bots")]
[Authorize]
public class BotsController : ControllerBase
{
    private readonly IBotRegistry _botRegistry;
    private readonly ILogger<BotsController> _logger;

    public BotsController(IBotRegistry botRegistry, ILogger<BotsController> logger)
    {
        _botRegistry = botRegistry;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<BotInfoDto>> GetAll()
    {
        // .Select() = .map() in JS. Transforms each bot into a DTO for the API response.
        var bots = _botRegistry.GetAllBots()
            .Select(b => new BotInfoDto(b.Name, b.Category, b.Description, b.SupportedContentTypes))
            .ToList();

        _logger.LogDebug("Retrieved {Count} bots", bots.Count);
        return Ok(bots);
    }

    /// <summary>
    /// Get a prompt template for a specific bot — use this in Claude Code
    /// to generate content matching the bot's format and style.
    /// </summary>
    [HttpGet("{botName}/prompt")]
    public ActionResult<PromptTemplateResponse> GetPromptTemplate(
        string botName,
        [FromQuery] string contentType = "Image",
        [FromQuery] string language = "en")
    {
        _logger.LogDebug("Requesting prompt template for bot '{BotName}', type '{ContentType}', language '{Language}'",
            botName, contentType, language);

        var bot = _botRegistry.GetBot(botName);
        if (bot is null)
        {
            _logger.LogWarning("Bot '{BotName}' not found", botName);
            return NotFound(new { error = $"Bot '{botName}' not found." });
        }

        if (!Enum.TryParse<ContentType>(contentType, true, out var parsed))
            return BadRequest(new { error = $"Invalid content type: {contentType}" });

        var prompt = bot.GetPromptTemplate(parsed, language);

        return Ok(new PromptTemplateResponse(
            bot.Name, bot.Category, contentType, language, prompt));
    }
}

public record PromptTemplateResponse(
    string BotName,
    string Category,
    string ContentType,
    string Language,
    string PromptTemplate);
