using HiLoGame.Mappers;
using HiLoGame.Models;
using HiLoGame.Models.Enums;
using HiLoGame.Models.Events;
using HiLoGame.Models.Responses;
using HiLoGame.Repositories;
using HiLoGame.Services;
using Microsoft.AspNetCore.SignalR;

namespace HiLoGame.Hubs;

public class GameHub : Hub
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMultiplayerGameService _gameService;
    private readonly IGameHistoryRepository _historyRepository;
    private readonly ILoggerService _logger;

    public GameHub(
        IRoomRepository roomRepository,
        IMultiplayerGameService gameService,
        IGameHistoryRepository historyRepository,
        ILoggerService logger)
    {
        _roomRepository = roomRepository;
        _gameService = gameService;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<CreateRoomResponse> CreateRoom(string roomName, string playerName, int minNumber, int maxNumber)
    {
        try
        {
            var range = GameRange.Create(minNumber, maxNumber);
            var player = Player.Create(playerName);
            
            var result = _gameService.CreateRoom(roomName, player, range);
            if (!result.IsSuccess)
            {
                return new CreateRoomResponse(Success: false, Error: result.ErrorMessage);
            }

            var room = result.Data!;
            
            // Set connection ID (infrastructure concern)
            room.Players[0].ConnectionId = Context.ConnectionId;
            
            _roomRepository.Save(room);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);

            // Notify all other clients that a new room is available
            await Clients.Others.SendAsync("RoomCreated", new RoomCreatedEvent(
                RoomId: room.RoomId,
                RoomName: room.RoomName,
                PlayersCount: room.Players.Count,
                MaxPlayers: room.MaxPlayers,
                CreatedAt: room.CreatedAt,
                MinNumber: room.Range.Min,
                MaxNumber: room.Range.Max
            ));

            return new CreateRoomResponse(
                Success: true,
                RoomId: room.RoomId,
                RoomName: room.RoomName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return new CreateRoomResponse(Success: false, Error: ex.Message);
        }
    }

    public async Task<JoinRoomResponse> JoinRoom(string roomId, string playerName)
    {
        try
        {
            var room = _roomRepository.GetById(roomId);
            if (room == null)
            {
                return new JoinRoomResponse(Success: false, Error: "Room not found");
            }

            var player = Player.Create(playerName);
            var joinResult = _gameService.JoinRoom(room, player);
            
            if (!joinResult.IsSuccess)
            {
                return new JoinRoomResponse(Success: false, Error: joinResult.ErrorMessage);
            }

            // Set connection ID (infrastructure concern)
            var roomPlayer = room.Players.Last();
            roomPlayer.ConnectionId = Context.ConnectionId;
            
            _roomRepository.Save(room);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Notify other players
            await Clients.OthersInGroup(roomId).SendAsync("PlayerJoined", new PlayerJoinedEvent(
                PlayerName: playerName,
                PlayersCount: room.Players.Count,
                MaxPlayers: room.MaxPlayers
            ));

            // If room is full, start the game
            if (_gameService.CanStartGame(room))
            {
                await StartGameAsync(roomId);
            }

            return new JoinRoomResponse(
                Success: true,
                RoomId: room.RoomId,
                RoomName: room.RoomName,
                PlayersCount: room.Players.Count,
                MaxPlayers: room.MaxPlayers
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return new JoinRoomResponse(Success: false, Error: ex.Message);
        }
    }

    private async Task StartGameAsync(string roomId)
    {
        var room = _roomRepository.GetById(roomId);
        if (room == null) return;

        var result = _gameService.StartGame(room);
        if (!result.IsSuccess) return;

        _roomRepository.Save(room);

        await Clients.Group(roomId).SendAsync("GameStarted", new GameStartedEvent(
            RoomName: room.RoomName,
            MinNumber: room.Range.Min,
            MaxNumber: room.Range.Max,
            Players: room.Players.Select(p => new PlayerInfo(
                Name: p.Player.Name,
                ConnectionId: p.ConnectionId
            )).ToList()
        ));
    }

    public async Task<MakeGuessResponse> MakeGuess(string roomId, int guess)
    {
        try
        {
            var room = _roomRepository.GetById(roomId);
            if (room == null)
            {
                return new MakeGuessResponse(Success: false, Error: "Room not found");
            }

            if (!room.IsStarted)
            {
                return new MakeGuessResponse(Success: false, Error: "Game not started");
            }

            if (_gameService.IsGameCompleted(room))
            {
                return new MakeGuessResponse(Success: false, Error: "Game already completed");
            }

            var roomPlayer = room.GetPlayer(Context.ConnectionId);
            if (roomPlayer == null)
            {
                return new MakeGuessResponse(Success: false, Error: "Player not found");
            }

            if (roomPlayer.HasGuessed)
            {
                return new MakeGuessResponse(Success: false, Error: "You already guessed this round. Wait for the other player.");
            }

            var result = _gameService.ProcessGuess(room, Context.ConnectionId, guess);

            if (!result.IsSuccess)
            {
                return new MakeGuessResponse(Success: false, Error: result.ErrorMessage);
            }

            // Notify all players that this player made a guess (but don't reveal the number or result)
            await Clients.Group(roomId).SendAsync("PlayerGuessed", new PlayerGuessedEvent(
                PlayerName: roomPlayer.Player.Name
            ));

            // If someone won, reveal everything
            if (result.Data?.IsGameWon == true)
            {
                // Save to history (like solo mode)
                var histories = GameRoomMapper.ToHistories(room);
                foreach (var history in histories)
                {
                    _historyRepository.Add(history);
                }

                await Clients.Group(roomId).SendAsync("GameCompleted", new GameCompletedEvent(
                    WinnerName: roomPlayer.Player.Name,
                    MysteryNumber: room.Game?.MysteryNumber ?? 0,
                    RoundNumber: room.CurrentRound,
                    Players: room.Game?.Players.Select(p => new PlayerGameInfo(
                        Name: p.Player.Name,
                        Attempts: p.Attempts,
                        IsWinner: p.IsWinner,
                        GuessAttempts: p.GuessAttempts.Select(a => new GuessAttemptInfo(
                            AttemptNumber: a.AttemptNumber,
                            GuessNumber: a.GuessNumber,
                            Result: a.Result.ToString()
                        )).ToList()
                    )).ToList() ?? new List<PlayerGameInfo>()
                ));
            }
            else
            {
                // Check if both players have guessed
                if (room.AllPlayersGuessed())
                {
                    // Start new round
                    room.StartNewRound();
                    _roomRepository.Save(room);

                    await Clients.Group(roomId).SendAsync("RoundCompleted", new RoundCompletedEvent(
                        RoundNumber: room.CurrentRound,
                        Message: "Both players guessed. New round!"
                    ));
                }
                else
                {
                    // This player has guessed, waiting for other player
                    var otherPlayer = room.Players.FirstOrDefault(p => p.ConnectionId != Context.ConnectionId);
                    
                    // Notify the player who just guessed that they need to wait
                    await Clients.Caller.SendAsync("WaitingForOtherPlayer", new WaitingForOtherPlayerEvent(
                        Message: $"Waiting for {otherPlayer?.Player.Name}..."
                    ));
                }
            }

            _roomRepository.Save(room);

            return new MakeGuessResponse(
                Success: true,
                Result: result.Data?.Result.ToString(),
                Message: result.Data?.Message,
                Attempts: result.Data?.Attempts ?? 0,
                IsWinner: result.Data?.IsGameWon ?? false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return new MakeGuessResponse(Success: false, Error: ex.Message);
        }
    }

    public Task<GetAvailableRoomsResponse> GetAvailableRooms()
    {
        try
        {
            var rooms = _roomRepository.GetAvailableRooms();
            return Task.FromResult(new GetAvailableRoomsResponse(
                Success: true,
                Rooms: rooms.Select(r => new RoomInfo(
                    RoomId: r.RoomId,
                    RoomName: r.RoomName,
                    PlayersCount: r.Players.Count,
                    MaxPlayers: r.MaxPlayers,
                    CreatedAt: r.CreatedAt,
                    MinNumber: r.Range.Min,
                    MaxNumber: r.Range.Max
                )).ToList()
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorCode.Unknown, ex.Message);
            return Task.FromResult(new GetAvailableRoomsResponse(Success: false, Rooms: new List<RoomInfo>(), Error: ex.Message));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var room = _roomRepository.FindByConnectionId(Context.ConnectionId);
        if (room != null)
        {
            var player = room.GetPlayer(Context.ConnectionId);
            var playerName = player?.Player.Name ?? "Unknown";

            room.Players.Remove(player!);
            
            // Delete room if empty, otherwise save
            if (room.Players.Count == 0)
            {
                _roomRepository.Delete(room.RoomId);
            }
            else
            {
                _roomRepository.Save(room);
            }

            // Notify other players
            await Clients.OthersInGroup(room.RoomId).SendAsync("PlayerLeft", new PlayerLeftEvent(
                PlayerName: playerName,
                Message: $"{playerName} has left the game"
            ));

            _logger.LogInfo($"Player {playerName} disconnected from room {room.RoomId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}

