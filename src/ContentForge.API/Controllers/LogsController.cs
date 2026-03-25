using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ContentForge.API.Controllers;

/// <summary>
/// Serves structured JSON logs from Serilog's CompactJsonFormatter files.
/// Reads NDJSON (newline-delimited JSON) from disk — like reading a JSON Lines file in Node.js.
/// </summary>
[ApiController]
[Route("api/logs")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;
    private readonly string _logDirectory;

    public LogsController(ILogger<LogsController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        // Resolve logs directory relative to the app's content root (where the API runs from).
        // In dev this is the project folder; in Docker it's /app/
        _logDirectory = Path.Combine(env.ContentRootPath, "logs");
    }

    /// <summary>
    /// Get paginated log entries with optional filters.
    /// Reads from the JSON log files (NDJSON format from CompactJsonFormatter).
    /// </summary>
    [HttpGet]
    public ActionResult GetLogs(
        [FromQuery] string? date = null,
        [FromQuery] string? level = null,
        [FromQuery] string? correlationId = null,
        [FromQuery] string? source = null,
        [FromQuery] string? search = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        try
        {
            var targetDate = date != null ? DateOnly.Parse(date) : DateOnly.FromDateTime(DateTime.Today);
            var logFile = GetJsonLogFilePath(targetDate);

            if (!System.IO.File.Exists(logFile))
            {
                return Ok(new LogPageResult([], 0, GetAvailableDates()));
            }

            // Read NDJSON file — each line is a complete JSON object.
            // Like fs.readFileSync().split('\n').map(JSON.parse) in Node.js.
            var allLines = System.IO.File.ReadAllLines(logFile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    try { return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(line); }
                    catch { return null; }
                })
                .Where(entry => entry != null)
                .Cast<Dictionary<string, JsonElement>>()
                .Reverse()  // Most recent first
                .ToList();

            // Apply filters
            var filtered = allLines.AsEnumerable();

            if (!string.IsNullOrEmpty(level))
            {
                filtered = filtered.Where(e =>
                    e.TryGetValue("@l", out var l) && l.GetString()?.Equals(level, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(correlationId))
            {
                filtered = filtered.Where(e =>
                    e.TryGetValue("CorrelationId", out var c) && c.GetString()?.Contains(correlationId, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(source))
            {
                filtered = filtered.Where(e =>
                    e.TryGetValue("SourceContext", out var s) && s.GetString()?.Contains(source, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(e =>
                    e.TryGetValue("@mt", out var m) && m.GetString()?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }

            var filteredList = filtered.ToList();
            var total = filteredList.Count;
            var page = filteredList.Skip(skip).Take(take).ToList();

            _logger.LogDebug("Served {Count}/{Total} log entries for {Date}", page.Count, total, targetDate);

            return Ok(new LogPageResult(page, total, GetAvailableDates()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read logs");
            return StatusCode(500, new { error = "Failed to read log files" });
        }
    }

    /// <summary>
    /// Get all log entries sharing the same CorrelationId — traces a single request
    /// across all middleware, commands, and repositories.
    /// </summary>
    [HttpGet("trace/{correlationId}")]
    public ActionResult TraceByCorrelationId(string correlationId, [FromQuery] string? date = null)
    {
        try
        {
            var targetDate = date != null ? DateOnly.Parse(date) : DateOnly.FromDateTime(DateTime.Today);
            var logFile = GetJsonLogFilePath(targetDate);

            if (!System.IO.File.Exists(logFile))
                return Ok(new { entries = Array.Empty<object>(), correlationId });

            var entries = System.IO.File.ReadAllLines(logFile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    try { return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(line); }
                    catch { return null; }
                })
                .Where(entry => entry != null
                    && entry.TryGetValue("CorrelationId", out var c)
                    && c.GetString()?.Equals(correlationId, StringComparison.OrdinalIgnoreCase) == true)
                .Cast<Dictionary<string, JsonElement>>()
                .ToList();

            _logger.LogDebug("Traced {Count} entries for CorrelationId {CorrelationId}", entries.Count, correlationId);

            return Ok(new { entries, correlationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trace logs for {CorrelationId}", correlationId);
            return StatusCode(500, new { error = "Failed to read log files" });
        }
    }

    /// <summary>
    /// Get summary statistics for today's or a given date's logs.
    /// </summary>
    [HttpGet("stats")]
    public ActionResult GetLogStats([FromQuery] string? date = null)
    {
        try
        {
            var targetDate = date != null ? DateOnly.Parse(date) : DateOnly.FromDateTime(DateTime.Today);
            var logFile = GetJsonLogFilePath(targetDate);

            if (!System.IO.File.Exists(logFile))
                return Ok(new LogStats(0, 0, 0, 0, 0));

            var lines = System.IO.File.ReadAllLines(logFile)
                .Where(line => !string.IsNullOrWhiteSpace(line));

            int total = 0, debug = 0, info = 0, warning = 0, error = 0;

            foreach (var line in lines)
            {
                total++;
                try
                {
                    var entry = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(line);
                    if (entry != null && entry.TryGetValue("@l", out var l))
                    {
                        switch (l.GetString())
                        {
                            case "Debug": debug++; break;
                            case "Information": info++; break;
                            case "Warning": warning++; break;
                            case "Error": error++; break;
                            default: info++; break; // CompactJsonFormatter omits @l for Information
                        }
                    }
                    else
                    {
                        info++; // Default level when @l is omitted = Information
                    }
                }
                catch { /* skip malformed lines */ }
            }

            return Ok(new LogStats(total, debug, info, warning, error));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute log stats");
            return StatusCode(500, new { error = "Failed to read log files" });
        }
    }

    private string GetJsonLogFilePath(DateOnly date)
    {
        // Serilog rolling file format: contentforge-json-YYYYMMDD.log
        return Path.Combine(_logDirectory, $"contentforge-json-{date:yyyyMMdd}.log");
    }

    private List<string> GetAvailableDates()
    {
        if (!Directory.Exists(_logDirectory))
            return [];

        return Directory.GetFiles(_logDirectory, "contentforge-json-*.log")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != null)
            .Select(name => name!.Replace("contentforge-json-", ""))
            .OrderDescending()
            .ToList();
    }
}

// DTOs as records — immutable value objects, like Object.freeze({}) in JS.
record LogPageResult(
    List<Dictionary<string, JsonElement>> Entries,
    int Total,
    List<string> AvailableDates);

record LogStats(int Total, int Debug, int Information, int Warning, int Error);
