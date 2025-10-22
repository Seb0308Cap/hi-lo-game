using HiLoGame.Models.Enums;
using HiLoGame.Models;

namespace HiLoGame.Services;

public interface ILoggerService
{
    void LogInfo(string message);
    void LogError(ErrorCode errorCode, string details);
    void LogError(ErrorCode errorCode, Exception exception);
    List<LogEntry> GetLogs();
}

