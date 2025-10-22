namespace HiLoGame.Models.Events;

public record PlayerLeftEvent(
    string PlayerName,
    string Message
);

