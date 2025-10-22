namespace HiLoGame.Models.Responses;

public record CreateRoomResponse(
    bool Success,
    string? RoomId = null,
    string? RoomName = null,
    string? Error = null
);

