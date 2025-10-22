using HiLoGame.Models;

namespace HiLoGame.Repositories;

public interface IRoomRepository
{
    GameRoom? GetById(string roomId);
    void Save(GameRoom room);
    void Delete(string roomId);
    List<GameRoom> GetAvailableRooms();
    GameRoom? FindByConnectionId(string connectionId);
}

