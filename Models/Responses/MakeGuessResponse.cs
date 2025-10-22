namespace HiLoGame.Models.Responses;

public record MakeGuessResponse(
    bool Success,
    string? Result = null,
    string? Message = null,
    int Attempts = 0,
    bool IsWinner = false,
    string? Error = null
);

