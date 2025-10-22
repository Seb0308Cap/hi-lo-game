using HiLoGame.Models.Enums;

namespace HiLoGame.Models.Exceptions;

public class GameException : Exception
{
    public ErrorCode ErrorCode { get; }

    public GameException(ErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public GameException(ErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

