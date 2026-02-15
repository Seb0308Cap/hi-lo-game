using HiLoGame.Models;
using HiLoGame.Models.Results;

namespace HiLoGame.Services;

public interface IMultiplayerGameService
{
    OperationResult<GameRoom> CreateRoom(string roomName, Player player, GameRange range, int totalGames = 1);
    OperationResult JoinRoom(GameRoom room, Player player);
    OperationResult<GameRoom> StartGame(GameRoom room);
    OperationResult<GuessResult> ProcessGuess(GameRoom room, string connectionId, int guess);
    /// <summary>Starts the next game in the room (new mystery number).</summary>
    OperationResult<GameRoom> StartNextGame(GameRoom room);
    bool CanStartGame(GameRoom room);
    bool IsGameCompleted(GameRoom room);
}

