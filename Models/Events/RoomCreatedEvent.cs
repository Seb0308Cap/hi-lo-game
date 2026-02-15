namespace HiLoGame.Models.Events;

public record RoomCreatedEvent(
    string RoomId,
    string RoomName,
    int PlayersCount,
    int MaxPlayers,
    DateTime CreatedAt,
    int MinNumber,
    int MaxNumber,
    int TotalGames
);

