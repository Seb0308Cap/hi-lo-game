using HiLoGame.Models;
using HiLoGame.Models.Results;

namespace HiLoGame.Services;

public interface IMultiplayerGameService
{
    OperationResult<GameRoom> CreateRoom(string roomName, Player player, GameRange range);
    OperationResult JoinRoom(GameRoom room, Player player);
    OperationResult<GameRoom> StartGame(GameRoom room);
    OperationResult<GuessResult> ProcessGuess(GameRoom room, string connectionId, int guess);
    bool CanStartGame(GameRoom room);
    bool IsGameCompleted(GameRoom room);
}

