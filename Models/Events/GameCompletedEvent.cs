namespace HiLoGame.Models.Events;

public record GameCompletedEvent(
    string? WinnerName,
    int MysteryNumber,
    int GamesPlayed,
    int TotalGames,
    List<PlayerScoreInfo> Scores,
    bool CanPlayAgain,
    List<PlayerGameInfo> Players
);

public record PlayerGameInfo(
    string Name,
    int Attempts,
    bool IsWinner,
    List<GuessAttemptInfo> GuessAttempts
);

public record GuessAttemptInfo(
    int AttemptNumber,
    int GuessNumber,
    string Result
);
