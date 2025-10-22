using HiLoGame.Models;
using System.Text.Json;

namespace HiLoGame.Repositories;

public class SessionGameRepository : IGameRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionKey = "GameState";

    public SessionGameRepository(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Game? GetCurrentGame()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return null;

        var sessionData = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(sessionData)) return null;

        return JsonSerializer.Deserialize<Game>(sessionData);
    }

    public void SaveGame(Game game)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return;

        var sessionData = JsonSerializer.Serialize(game);
        session.SetString(SessionKey, sessionData);
    }

    public void ClearGame()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(SessionKey);
    }
}

