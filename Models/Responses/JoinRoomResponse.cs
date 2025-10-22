namespace HiLoGame.Models.Responses;

public record JoinRoomResponse(
    bool Success,
    string? RoomId = null,
    string? RoomName = null,
    int PlayersCount = 0,
    int MaxPlayers = 0,
    string? Error = null
);

