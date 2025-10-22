using HiLoGame.Models.Enums;
using HiLoGame.Models;
using HiLoGame.Models.Results;

namespace HiLoGame.Services;

public interface IGameService
{
    OperationResult<Game> CreateNewGame(GameMode mode, Player player, GameRange range);
    OperationResult<GuessResult> ProcessGuess(Game game, int guess);
}

