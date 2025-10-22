using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int MysteryNumber { get; set; }
    public GameRange Range { get; set; } = null!;
    public GameMode Mode { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public List<PlayerGameState> Players { get; set; } = new();

    public PlayerGameState? Winner => Players.FirstOrDefault(p => p.IsWinner);
    public PlayerGameState? CurrentPlayer => Players.FirstOrDefault();
    public int MinNumber => Range.Min;
    public int MaxNumber => Range.Max;

    public static Game Create(GameMode mode, Player player, GameRange range, int mysteryNumber)
    {
        var game = new Game
        {
            Range = range,
            Mode = mode,
            MysteryNumber = mysteryNumber,
            IsCompleted = false
        };

        game.AddPlayer(player);
        return game;
    }

    public void AddPlayer(Player player)
    {
        Players.Add(PlayerGameState.Create(player));
    }

    public GameResult DetermineResult(int guess)
    {
        if (guess == MysteryNumber) return GameResult.Win;
        return guess < MysteryNumber ? GameResult.Higher : GameResult.Lower;
    }

    public void CompleteGame()
    {
        if (CurrentPlayer != null)
        {
            CurrentPlayer.MarkAsWinner();
        }
        IsCompleted = true;
        CompletedAt = DateTime.Now;
    }
}

