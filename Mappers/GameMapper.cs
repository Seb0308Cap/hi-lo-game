using HiLoGame.Models;

namespace HiLoGame.Mappers;

public static class GameMapper
{
    public static GameHistory ToHistory(Game game)
    {
        var winner = game.Winner!;

        return new GameHistory
        {
            PlayerName = winner.Player.Name,
            Attempts = winner.Attempts,
            CompletedAt = game.CompletedAt ?? DateTime.Now,
            MinNumber = game.MinNumber,
            MaxNumber = game.MaxNumber,
            MysteryNumber = game.MysteryNumber,
            GuessAttempts = winner.GuessAttempts.ToList()
        };
    }
}

