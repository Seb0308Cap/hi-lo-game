namespace HiLoGame.Models.Events;

public record RoundCompletedEvent(
    int RoundNumber,
    string Message
);

