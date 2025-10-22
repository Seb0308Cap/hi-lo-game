using HiLoGame.Models;

namespace HiLoGame.Repositories;

public interface IGameHistoryRepository
{
    void Add(GameHistory history);
    List<GameHistory> GetAll();
}

