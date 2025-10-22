using HiLoGame.Models.Enums;
using HiLoGame.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HiLoGame.Services;

public class BrowserLoggerService : ILoggerService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BrowserLoggerService> _logger;
    private const string LogCacheKey = "BrowserLogs";
    private const int MaxLogs = 100;

    public BrowserLoggerService(IMemoryCache cache, ILogger<BrowserLoggerService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        AddLog("INFO", message);
    }

    public void LogError(ErrorCode errorCode, string details)
    {
        var logMessage = $"[{errorCode}] {details}";
        _logger.LogError(logMessage);
        AddLog("ERROR", logMessage);
    }

    public void LogError(ErrorCode errorCode, Exception exception)
    {
        _logger.LogError(exception, "[{ErrorCode}]", errorCode);
        AddLog("ERROR", $"[{errorCode}] {exception.Message}");
    }

    public List<LogEntry> GetLogs()
    {
        return _cache.GetOrCreate(LogCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            return new List<LogEntry>();
        }) ?? new List<LogEntry>();
    }

    private void AddLog(string level, string message)
    {
        var logs = GetLogs();
        logs.Insert(0, new LogEntry(DateTime.Now, level, message));

        if (logs.Count > MaxLogs)
        {
            logs = logs.Take(MaxLogs).ToList();
        }

        _cache.Set(LogCacheKey, logs, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        });
    }
}

