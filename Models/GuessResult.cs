using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public record GuessResult(
    GameResult Result,
    string Message,
    int Attempts,
    bool IsGameWon
);

