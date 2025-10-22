using HiLoGame.Models;

namespace HiLoGame.Mappers;

public static class GameRoomMapper
{
    public static List<GameHistory> ToHistories(GameRoom room)
    {
        if (room.Game == null || !room.IsCompleted)
        {
            return new List<GameHistory>();
        }

        return room.Game.Players
            .Select(playerState => new GameHistory
            {
                PlayerName = playerState.Player.Name,
                Attempts = playerState.Attempts,
                CompletedAt = room.Game.CompletedAt ?? DateTime.Now,
                MinNumber = room.Game.MinNumber,
                MaxNumber = room.Game.MaxNumber,
                MysteryNumber = room.Game.MysteryNumber,
                GuessAttempts = playerState.GuessAttempts.ToList()
            })
            .ToList();
    }
}

