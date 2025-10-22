namespace HiLoGame.Models;

public record LogEntry(
    DateTime Timestamp,
    string Level,
    string Message
);

