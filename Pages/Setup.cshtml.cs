using HiLoGame.Models.Constants;
using HiLoGame.Models.Enums;
using HiLoGame.Models.Exceptions;
using HiLoGame.Models;
using HiLoGame.Repositories;
using HiLoGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiLoGame.Pages;

public class SetupModel : PageModel
{
    private readonly IGameService _gameService;
    private readonly IGameRepository _gameRepository;

    public string? ErrorMessage { get; set; }
    public int DefaultMinNumber => GameConstants.MinNumber;
    public int DefaultMaxNumber => GameConstants.MaxNumber;

    public SetupModel(IGameService gameService, IGameRepository gameRepository)
    {
        _gameService = gameService;
        _gameRepository = gameRepository;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPostStartGame(string playerName, int minNumber, int maxNumber)
    {
        try
        {
            var player = Player.Create(playerName);
            var range = GameRange.Create(minNumber, maxNumber);
            var result = _gameService.CreateNewGame(GameMode.Solo, player, range);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }

            _gameRepository.SaveGame(result.Data!);
            return RedirectToPage("/Solo");
        }
        catch (GameException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}

