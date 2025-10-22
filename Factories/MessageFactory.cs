using HiLoGame.Models.Constants;
using HiLoGame.Models.Enums;

namespace HiLoGame.Factories;

public static class MessageFactory
{
    public static string CreateMessage(GameResult result, int mysteryNumber)
    {
        return result switch
        {
            GameResult.Win => string.Format(MessageConstants.Win, mysteryNumber),
            GameResult.Higher => MessageConstants.Higher,
            GameResult.Lower => MessageConstants.Lower,
            _ => string.Empty
        };
    }

    public static string GetErrorMessage(ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.InvalidPlayerName => "Player name cannot be empty",
            ErrorCode.InvalidRange => "Min must be less than Max",
            ErrorCode.RangeTooSmall => "Range must be at least 3 numbers",
            ErrorCode.GuessOutOfRange => "Guess is out of the valid range",
            ErrorCode.NoActivePlayer => "No active player in the game",
            ErrorCode.GameAlreadyCompleted => "Game is already completed",
            ErrorCode.GameNotCompleted => "Game must be completed first",
            _ => "An unknown error occurred"
        };
    }

    public static string GetErrorMessageWithDetails(ErrorCode errorCode, params object[] args)
    {
        var baseMessage = GetErrorMessage(errorCode);

        return errorCode switch
        {
            ErrorCode.GuessOutOfRange when args.Length >= 2
                => $"{baseMessage}. Valid range: [{args[0]} - {args[1]}]",
            _ => baseMessage
        };
    }
}

