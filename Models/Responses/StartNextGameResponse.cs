namespace HiLoGame.Models.Responses;

public record StartNextGameResponse(
    bool Success,
    string? Error = null,
    bool AllReady = false,
    List<string>? WaitingPlayerNames = null
);
