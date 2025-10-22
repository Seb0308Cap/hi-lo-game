using HiLoGame.Models.Enums;
using HiLoGame.Models.Exceptions;
using HiLoGame.Factories;
using HiLoGame.Models;
using HiLoGame.Models.Results;
using HiLoGame.Utils;

namespace HiLoGame.Services;

public class GameService : IGameService
{
    private readonly IRandomNumberGenerator _randomGenerator;
    private readonly ILoggerService _logger;

    public GameService(IRandomNumberGenerator randomGenerator, ILoggerService logger)
    {
        _randomGenerator = randomGenerator;
        _logger = logger;
    }

    public OperationResult<Game> CreateNewGame(GameMode mode, Player player, GameRange range)
    {
        try
        {
            var mysteryNumber = _randomGenerator.Next(range.Min, range.Max + 1);
            var game = Game.Create(mode, player, range, mysteryNumber);

            _logger.LogInfo($"New game created for player {player.Name} with range {range}");

            return OperationResult<Game>.Success(game);
        }
        catch (GameException ex)
        {
            _logger.LogError(ex.ErrorCode, ex.Message);
            return OperationResult<Game>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    public OperationResult<GuessResult> ProcessGuess(Game game, int guess)
    {
        try
        {
            var player = game.CurrentPlayer;

            if (player == null)
            {
                var errorCode = ErrorCode.NoActivePlayer;
                var message = MessageFactory.GetErrorMessage(errorCode);
                _logger.LogError(errorCode, $"Game ID: {game.Id}");
                return OperationResult<GuessResult>.Failure(errorCode, message);
            }

            if (!game.Range.IsInRange(guess))
            {
                var errorCode = ErrorCode.GuessOutOfRange;
                var message = MessageFactory.GetErrorMessageWithDetails(errorCode, game.Range.Min, game.Range.Max);
                _logger.LogError(errorCode, $"Guess: {guess}, Range: {game.Range}");
                return OperationResult<GuessResult>.Failure(errorCode, message);
            }

            var gameResult = game.DetermineResult(guess);
            var resultMessage = MessageFactory.CreateMessage(gameResult, game.MysteryNumber);

            player.RecordAttempt(guess, gameResult, resultMessage);

            if (gameResult == GameResult.Win)
            {
                game.CompleteGame();
                _logger.LogInfo($"Player {player.Player.Name} won in {player.Attempts} attempts");
            }

            var result = new GuessResult(gameResult, resultMessage, player.Attempts, gameResult == GameResult.Win);
            return OperationResult<GuessResult>.Success(result);
        }
        catch (GameException ex)
        {
            _logger.LogError(ex.ErrorCode, ex.Message);
            return OperationResult<GuessResult>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

