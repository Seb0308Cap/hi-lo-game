namespace HiLoGame.Models.Events;

public record GameStartedEvent(
    string RoomName,
    int MinNumber,
    int MaxNumber,
    List<PlayerInfo> Players
);

public record PlayerInfo(
    string Name,
    string? ConnectionId
);

