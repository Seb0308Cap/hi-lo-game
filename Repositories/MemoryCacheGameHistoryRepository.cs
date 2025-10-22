using HiLoGame.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HiLoGame.Repositories;

public class MemoryCacheGameHistoryRepository : IGameHistoryRepository
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "GameHistory";
    private const int MaxHistorySize = 50;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(30);

    public MemoryCacheGameHistoryRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Add(GameHistory history)
    {
        var historyList = GetAll();
        historyList.Insert(0, history);
        _cache.Set(CacheKey, historyList, CacheExpiration);
    }

    public List<GameHistory> GetAll()
    {
        var allHistory = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            return new List<GameHistory>();
        }) ?? new List<GameHistory>();

        return allHistory.Take(MaxHistorySize).ToList();
    }
}

