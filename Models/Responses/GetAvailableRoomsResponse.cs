namespace HiLoGame.Models.Responses;

public record GetAvailableRoomsResponse(
    bool Success,
    List<RoomInfo> Rooms,
    string? Error = null
)
{
    // Parameterless constructor for easier instantiation
    public GetAvailableRoomsResponse() : this(false, new List<RoomInfo>(), null)
    {
    }
};

public record RoomInfo(
    string RoomId,
    string RoomName,
    int PlayersCount,
    int MaxPlayers,
    DateTime CreatedAt,
    int MinNumber,
    int MaxNumber,
    int TotalGames
);

