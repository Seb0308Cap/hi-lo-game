namespace HiLoGame.Models.Events;

public record PlayerJoinedEvent(
    string PlayerName,
    int PlayersCount,
    int MaxPlayers
);

