namespace HiLoGame.Models.Events;

public record RoundCompletedEvent(
    int RoundNumber,
    string Message
);

public record PlayerScoreInfo(string Name, int Wins);
