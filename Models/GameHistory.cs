using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public record GameHistory
{
    public required string PlayerName { get; init; }
    public required int Attempts { get; init; }
    public required DateTime CompletedAt { get; init; }
    public required int MinNumber { get; init; }
    public required int MaxNumber { get; init; }
    public required int MysteryNumber { get; init; }
    public required List<GuessAttempt> GuessAttempts { get; init; }
    public required GameMode Mode { get; init; }
}
