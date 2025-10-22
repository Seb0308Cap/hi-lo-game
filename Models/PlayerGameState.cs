using HiLoGame.Models.Enums;

namespace HiLoGame.Models;

public class PlayerGameState
{
    public Player Player { get; set; } = null!;
    public int Attempts { get; set; }
    public List<GuessAttempt> GuessAttempts { get; set; } = new();
    public bool IsWinner { get; set; }
    public string? LastMessage { get; set; }

    public static PlayerGameState Create(Player player)
    {
        return new PlayerGameState
        {
            Player = player,
            Attempts = 0,
            IsWinner = false
        };
    }

    public void RecordAttempt(int guess, GameResult result, string message)
    {
        Attempts++;
        GuessAttempts.Add(new GuessAttempt(guess, result, Attempts));
        LastMessage = message;
    }

    public void MarkAsWinner()
    {
        IsWinner = true;
    }
}

