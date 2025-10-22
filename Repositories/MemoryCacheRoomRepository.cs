using System.Collections.Concurrent;
using HiLoGame.Models;

namespace HiLoGame.Repositories;

public class MemoryCacheRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

    public GameRoom? GetById(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public void Save(GameRoom room)
    {
        _rooms[room.RoomId] = room;
    }

    public void Delete(string roomId)
    {
        _rooms.TryRemove(roomId, out _);
    }

    public List<GameRoom> GetAvailableRooms()
    {
        return _rooms.Values
            .Where(r => !r.IsFull && !r.IsStarted)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public GameRoom? FindByConnectionId(string connectionId)
    {
        return _rooms.Values.FirstOrDefault(r => r.GetPlayer(connectionId) != null);
    }
}

