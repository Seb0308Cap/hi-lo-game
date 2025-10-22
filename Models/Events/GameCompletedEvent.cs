namespace HiLoGame.Models.Events;

public record GameCompletedEvent(
    string WinnerName,
    int MysteryNumber,
    int RoundNumber,
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

