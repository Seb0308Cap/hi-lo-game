using HiLoGame.Models;

namespace HiLoGame.Repositories;

public interface IGameRepository
{
    Game? GetCurrentGame();
    void SaveGame(Game game);
    void ClearGame();
}

