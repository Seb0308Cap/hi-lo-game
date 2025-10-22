namespace HiLoGame.Utils;

public class RandomNumberGenerator : IRandomNumberGenerator
{
    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue);
    }
}

