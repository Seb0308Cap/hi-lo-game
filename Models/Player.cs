using HiLoGame.Models.Enums;
using HiLoGame.Models.Exceptions;
using HiLoGame.Factories;

namespace HiLoGame.Models;

public class Player
{
    public string Name { get; set; } = string.Empty;

    public static Player Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var message = MessageFactory.GetErrorMessage(ErrorCode.InvalidPlayerName);
            throw new GameException(ErrorCode.InvalidPlayerName, message);
        }

        return new Player { Name = name.Trim() };
    }
}

