using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public class GameRoom
{
    public string RoomId { get; set; } = Guid.NewGuid().ToString();
    public string RoomName { get; set; } = string.Empty;
    public Game? Game { get; set; }
    public GameRange Range { get; set; } = null!;
    public List<RoomPlayer> Players { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public int MaxPlayers { get; set; } = 2;
    public int CurrentRound { get; set; } = 1;

    public RoomPlayer? GetPlayer(string connectionId)
    {
        return Players.FirstOrDefault(p => p.ConnectionId == connectionId);
    }

    public bool IsFull => Players.Count >= MaxPlayers;

    public bool CanStart => Players.Count == MaxPlayers && !IsStarted;

    public void StartGame(int mysteryNumber)
    {
        Game = new Game
        {
            MysteryNumber = mysteryNumber,
            Range = Range,
            Mode = GameMode.Multiplayer,
            IsCompleted = false
        };

        foreach (var player in Players)
        {
            Game.AddPlayer(player.Player);
        }

        IsStarted = true;
        CurrentRound = 1;
    }

    public bool AllPlayersGuessed()
    {
        return Players.All(p => p.HasGuessed);
    }

    public void StartNewRound()
    {
        CurrentRound++;
        foreach (var player in Players)
        {
            player.ResetGuess();
        }
    }

    public PlayerGameState? GetPlayerGameState(string connectionId)
    {
        var player = GetPlayer(connectionId);
        if (player == null || Game == null) return null;
        
        return Game.Players.FirstOrDefault(p => p.Player.Name == player.Player.Name);
    }

    public void CompleteGame()
    {
        IsCompleted = true;
        if (Game != null)
        {
            Game.IsCompleted = true;
            Game.CompletedAt = DateTime.Now;
        }
    }
}

