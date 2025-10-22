namespace HiLoGame.Models;

public class RoomPlayer
{
    public string ConnectionId { get; set; } = string.Empty;
    public Player Player { get; set; } = null!;
    public bool IsReady { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool HasGuessed { get; set; }

    public static RoomPlayer Create(string connectionId, Player player)
    {
        return new RoomPlayer
        {
            ConnectionId = connectionId,
            Player = player,
            IsReady = false,
            HasGuessed = false
        };
    }

    public void ResetGuess()
    {
        HasGuessed = false;
    }
}

