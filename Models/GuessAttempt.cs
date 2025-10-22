using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public record GuessAttempt(
    int GuessNumber,
    GameResult Result,
    int AttemptNumber
);

