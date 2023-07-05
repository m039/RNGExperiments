using System;

namespace RNGExperiments;

public enum RngType
{
    LCG, SystemDefault, Xorshift32, Xorshift128
}

public static class RngTypeExt
{
    public static string GetDescription(this RngType type)
    {
        return type switch
        {
            RngType.SystemDefault => "System.Random",
            _ => type.ToString(),
        };
    }

    public static IRng Create(this RngType type, int seed)
    {
        return type switch
        {
            RngType.Xorshift128 => new Xorshift128(seed),
            RngType.SystemDefault => new SystemRng(seed),
            RngType.Xorshift32 => new Xorshift32((uint) seed),
            _ => new LCG((uint)seed),
        };
    }

    public static bool CanCheckWhenRepeats(this RngType type)
    {
        return type switch
        {
            RngType.LCG => true,
            RngType.Xorshift32 => true,
            _ => false,
        };
    }
}

public interface IRng
{
    double Random();

    bool IsStartedRepeat();
}

class SystemRng : IRng
{
    readonly Random _random;

    public SystemRng(int seed)
    {
        _random = new Random(seed);
    }

    public bool IsStartedRepeat()
    {
        throw new NotSupportedException();
    }

    public double Random()
    {
        return _random.NextDouble();
    }
}

class LCG : IRng
{
    const uint Modulus = (uint)1 << 31;
    const uint Multiplier = 1103515245;
    const uint Increment = 12345;

    uint _seed;

    readonly uint _initSeed;

    bool _isStartedRepeat;

    public LCG(uint seed)
    {
        _seed = seed;
        _initSeed = seed;
    }

    public bool IsStartedRepeat()
    {
        return _isStartedRepeat;
    }

    public double Random()
    {
        _seed = (Multiplier * _seed + Increment) % Modulus;
        if (_seed == _initSeed)
        {
            _isStartedRepeat = true;
        }
        return _seed / (double)Modulus;
    }
}

class Xorshift32 : IRng
{
    readonly uint _initSeed;

    uint _state;

    bool _isStartedRepeat;

    public Xorshift32(uint seed) {
        if (seed == 0) {
            seed = 1;
        }

        _state = seed;
        _initSeed = seed;
    }

    public bool IsStartedRepeat()
    {
        return _isStartedRepeat;
    }

    public double Random()
    {
        var x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        _state = x;

        if (_state == _initSeed) {
            _isStartedRepeat = true;
        }

        return _state / (double) uint.MaxValue;
    }
}

class Xorshift128 : IRng
{
    int x0, x1, x2, x3;

    readonly int _initSeed;

    bool _isStartedRepeat;

    public Xorshift128(int seed)
    {
        if (seed == 0) {
            seed = 1;
        }

        x3 = seed;
        _initSeed = seed;
    }

    public bool IsStartedRepeat()
    {
        return _isStartedRepeat;
    }

    public double Random()
    {
        var t = x3;

        var s = x0;
        x3 = x2;
        x2 = x1;
        x1 = s;

        t ^= t << 11;
        t ^= t >> 8;

        x0 = t ^ s ^ (s >> 19);

        if (x3 == _initSeed && x2 == 0 && x1 == 0 && x0 == 0)
        {
            _isStartedRepeat = true;
        }

        return x0 / (double)int.MaxValue;
    }
}
