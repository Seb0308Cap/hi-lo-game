using HiLoGame.Models.Enums;
using HiLoGame.Models.Exceptions;
using HiLoGame.Factories;
using HiLoGame.Models;
using HiLoGame.Models.Results;
using HiLoGame.Utils;

namespace HiLoGame.Services;

public class MultiplayerGameService : IMultiplayerGameService
{
    private readonly IRandomNumberGenerator _randomGenerator;
    private readonly ILoggerService _logger;

    public MultiplayerGameService(IRandomNumberGenerator randomGenerator, ILoggerService logger)
    {
        _randomGenerator = randomGenerator;
        _logger = logger;
    }

    public OperationResult<GameRoom> CreateRoom(string roomName, Player player, GameRange range)
    {
        try
        {
            var room = new GameRoom
            {
                RoomName = roomName,
                Range = range
            };

            var roomPlayer = RoomPlayer.Create(string.Empty, player); // ConnectionId will be set by Hub
            room.Players.Add(roomPlayer);

            _logger.LogInfo($"Room '{roomName}' created by {player.Name} with range [{range.Min}-{range.Max}]");
            
            return OperationResult<GameRoom>.Success(room);
        }
        catch (GameException ex)
        {
            _logger.LogError(ex.ErrorCode, ex.Message);
            return OperationResult<GameRoom>.Failure(ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return OperationResult<GameRoom>.Failure(ErrorCode.Unknown, "Failed to create room");
        }
    }

    public OperationResult JoinRoom(GameRoom room, Player player)
    {
        try
        {
            if (room.IsFull)
            {
                return OperationResult.Failure(ErrorCode.Unknown, "Room is full");
            }

            if (room.IsStarted)
            {
                return OperationResult.Failure(ErrorCode.Unknown, "Game already started");
            }

            // Prevent duplicate player names in the same room
            if (room.Players.Any(p => string.Equals(p.Player.Name, player.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return OperationResult.Failure(ErrorCode.DuplicatePlayerName, $"A player named '{player.Name}' is already in this room");
            }

            var roomPlayer = RoomPlayer.Create(string.Empty, player); // ConnectionId will be set by Hub
            room.Players.Add(roomPlayer);

            _logger.LogInfo($"Player {player.Name} joined room {room.RoomId}");
            
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return OperationResult.Failure(ErrorCode.Unknown, "Failed to join room");
        }
    }

    public OperationResult<GameRoom> StartGame(GameRoom room)
    {
        try
        {
            if (!CanStartGame(room))
            {
                return OperationResult<GameRoom>.Failure(ErrorCode.Unknown, "Cannot start game");
            }

            var mysteryNumber = _randomGenerator.Next(room.Range.Min, room.Range.Max + 1);
            room.StartGame(mysteryNumber);

            _logger.LogInfo($"Game started in room {room.RoomId} with mystery number in range [{room.Range.Min}-{room.Range.Max}]");
            
            return OperationResult<GameRoom>.Success(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return OperationResult<GameRoom>.Failure(ErrorCode.Unknown, "Failed to start game");
        }
    }

    public OperationResult<GuessResult> ProcessGuess(GameRoom room, string connectionId, int guess)
    {
        try
        {
            if (room.Game == null)
            {
                return OperationResult<GuessResult>.Failure(ErrorCode.GameNotFound, "Game not found");
            }

            var roomPlayer = room.GetPlayer(connectionId);
            if (roomPlayer == null)
            {
                return OperationResult<GuessResult>.Failure(ErrorCode.NoActivePlayer, "Player not found");
            }

            var playerGameState = room.GetPlayerGameState(connectionId);
            if (playerGameState == null)
            {
                return OperationResult<GuessResult>.Failure(ErrorCode.NoActivePlayer, "Player state not found");
            }

            if (!room.Game.Range.IsInRange(guess))
            {
                var errorCode = ErrorCode.GuessOutOfRange;
                var message = MessageFactory.GetErrorMessageWithDetails(errorCode, room.Game.Range.Min, room.Game.Range.Max);
                _logger.LogError(errorCode, $"Guess: {guess}, Range: {room.Game.Range}");
                return OperationResult<GuessResult>.Failure(errorCode, message);
            }

            var gameResult = room.Game.DetermineResult(guess);
            var resultMessage = MessageFactory.CreateMessage(gameResult, room.Game.MysteryNumber);

            playerGameState.RecordAttempt(guess, gameResult, resultMessage);
            roomPlayer.HasGuessed = true;

            if (gameResult == GameResult.Win)
            {
                playerGameState.MarkAsWinner();
                room.CompleteGame();
                _logger.LogInfo($"Player {roomPlayer.Player.Name} won in room {room.RoomId}");
            }

            var result = new GuessResult(gameResult, resultMessage, playerGameState.Attempts, gameResult == GameResult.Win);
            return OperationResult<GuessResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return OperationResult<GuessResult>.Failure(ErrorCode.Unknown, "An error occurred");
        }
    }

    public bool CanStartGame(GameRoom room)
    {
        return room.CanStart;
    }

    public bool IsGameCompleted(GameRoom room)
    {
        return room.IsCompleted;
    }
}

