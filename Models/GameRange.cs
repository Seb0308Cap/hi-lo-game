using HiLoGame.Factories;
using HiLoGame.Models.Enums;
using HiLoGame.Models.Exceptions;

namespace HiLoGame.Models;

public record GameRange(int Min, int Max)
{
    public static GameRange Create(int min, int max)
    {
        if (min >= max)
        {
            var message = MessageFactory.GetErrorMessage(ErrorCode.InvalidRange);
            throw new GameException(ErrorCode.InvalidRange, message);
        }

        if (max - min < 2)
        {
            var message = MessageFactory.GetErrorMessage(ErrorCode.RangeTooSmall);
            throw new GameException(ErrorCode.RangeTooSmall, message);
        }

        return new GameRange(min, max);
    }

    public bool IsInRange(int value) => value >= Min && value <= Max;

    public override string ToString() => $"[{Min} - {Max}]";
}

