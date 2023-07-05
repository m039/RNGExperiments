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
            RngType.Xorshift128 => new Xorshift128((uint)seed),
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

    uint RandomUInt();
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

    public uint RandomUInt() {
        uint thirtyBits = (uint) _random.Next(1 << 30);
        uint twoBits = (uint) _random.Next(1 << 2);
        uint fullRange = (thirtyBits << 2) | twoBits;
        return fullRange;
    }
}

class LCG : IRng
{
    const uint Multiplier = 134775813;
    const uint Increment = 1;

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
        return RandomUInt() / (double)uint.MaxValue;
    }

    public uint RandomUInt() {
        _seed = Multiplier * _seed + Increment;
        if (_seed == _initSeed)
        {
            _isStartedRepeat = true;
        }
        return _seed;
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
        return RandomUInt() / (double) uint.MaxValue;
    }

    public uint RandomUInt() {
        var x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        _state = x;

        if (_state == _initSeed) {
            _isStartedRepeat = true;
        }

        return _state;
    }
}

class Xorshift128 : IRng
{
    uint x0, x1, x2, x3;

    readonly uint _initSeed;

    bool _isStartedRepeat;

    public Xorshift128(uint seed)
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
        return RandomUInt() / (double)int.MaxValue;
    }

    public uint RandomUInt() {
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

        return x0;
    }
}
