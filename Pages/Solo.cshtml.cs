using HiLoGame.Mappers;
using HiLoGame.Models;
using HiLoGame.Repositories;
using HiLoGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HiLoGame.Models.Enums;

namespace HiLoGame.Pages;

public class SoloModel : PageModel
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameService _gameService;
    private readonly IGameHistoryRepository _historyRepository;
    private readonly ILoggerService _logger;

    public Game? CurrentGame { get; set; }
    public string? ErrorMessage { get; set; }
    public List<GameHistory> GameHistory { get; set; } = new();
    public List<LogEntry> Logs { get; set; } = new();

    public SoloModel(IGameRepository gameRepository, IGameService gameService, IGameHistoryRepository historyRepository, ILoggerService logger)
    {
        _gameRepository = gameRepository;
        _gameService = gameService;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        CurrentGame = _gameRepository.GetCurrentGame();

        if (CurrentGame == null)
        {
            return RedirectToPage("/Setup");
        }

        GameHistory = _historyRepository.GetAll()
            .Where(h => h.Mode == GameMode.Solo)
            .ToList();
        Logs = _logger.GetLogs();
        return Page();
    }

    public IActionResult OnPostNewGame()
    {
        _gameRepository.ClearGame();
        return RedirectToPage("/Setup");
    }

    public IActionResult OnPostMakeGuess(int guess)
    {
        CurrentGame = _gameRepository.GetCurrentGame();

        if (CurrentGame == null || CurrentGame.IsCompleted)
        {
            return RedirectToPage();
        }

        var result = _gameService.ProcessGuess(CurrentGame, guess);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            GameHistory = _historyRepository.GetAll()
                .Where(h => h.Mode == GameMode.Solo)
                .ToList();
            Logs = _logger.GetLogs();
            return Page();
        }

        if (result.Data!.IsGameWon)
        {
            _historyRepository.Add(GameMapper.ToHistory(CurrentGame));
        }

        _gameRepository.SaveGame(CurrentGame);
        return RedirectToPage();
    }
}

