using System;

namespace RNGExperiments;

public enum RngType
{
    LCG, SystemDefault
}

public static class RngTypeExt
{
    public static IRng Create(this RngType type, int seed)
    {
        switch (type)
        {
            case RngType.SystemDefault:
                return new SystemRng(seed);
            default:
            case RngType.LCG:
                return new LCG(seed);
        }
    }
}

public interface IRng
{
    double Random();
}

class SystemRng : IRng
{
    Random _random;

    public SystemRng(int seed)
    {
        _random = new Random(seed);
    }

    public double Random()
    {
        return _random.NextDouble();
    }
}

class LCG : IRng
{
    const int Modulus = 1 << 31;
    const int Multiplier = 1103515245;
    const int Increment = 12345;

    int _seed;

    public LCG(int seed)
    {
        _seed = seed;
    }

    public double Random()
    {
        _seed = (Multiplier * _seed + Increment) % Modulus;
        return _seed / (double)Modulus;
    }
}