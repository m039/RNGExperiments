using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using Avalonia;
using System;
using ReactiveUI;
using Avalonia.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace RNGExperiments;

public class MainWindowViewModel : ViewModelBase
{
    public enum RngType
    {
        LCG, SystemDefault
    }

    int _imageWidth = 3000;

    int _imageHeight = 3000;

    int _rngSeed = 0;

    RngType _rngType = RngType.LCG;

    Bitmap? _bitmap;

    IImage? _imageSource;

    string _infoText = string.Empty;

    bool _isReady;

    bool _isGenerating;

    CancellationTokenSource? _cancellationToken;

    public MainWindowViewModel()
    {
        var labels = new List<RngGeneratorLabel>();

        foreach (RngType rngType in Enum.GetValues(typeof(RngType)))
        {
            labels.Add(new RngGeneratorLabel(
                rngType.ToString(),
                rngType
            ));
        }

        RngGeneratorLabels = labels;
    }

    public void Ready()
    {
        _isReady = true;
        SetImage();
    }

    public int ImageWidth
    {
        get => _imageWidth;
        set
        {
            if (value <= 0)
            {
                throw new DataValidationException("Only positive numbers greater than zero are allowed.");
            }

            this.RaiseAndSetIfChanged(ref _imageWidth, Math.Max(value, 1));
            SetImage();
        }
    }

    public int ImageHeight
    {
        get => _imageHeight;
        set
        {
            if (value <= 0)
            {
                throw new DataValidationException("Only positive numbers greater than zero are allowed.");
            }

            this.RaiseAndSetIfChanged(ref _imageHeight, Math.Max(value, 1));
            SetImage();
        }
    }

    public IImage? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    public string InfoText
    {
        get => _infoText;
        set => this.RaiseAndSetIfChanged(ref _infoText, value);
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        set => this.RaiseAndSetIfChanged(ref _isGenerating, value);
    }

    public int RngSeed
    {
        get => _rngSeed;
        set
        {
            this.RaiseAndSetIfChanged(ref _rngSeed, value);
            SetImage();
        }
    }

    public RngGeneratorLabel SelectedRngItem
    {
        set
        {
            _rngType = value.RngType;
            SetImage();
        }
    }

    public IEnumerable<RngGeneratorLabel> RngGeneratorLabels { get; }

    async void SetImage()
    {
        if (!_isReady)
        {
            return;
        }

        _cancellationToken?.Cancel();
        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;

        ImageSource = null;
        IsGenerating = true;
        InfoText = "Generating...";

        if (_bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }

        var stopWatch = new Stopwatch();

        var pixelFormat = PixelFormat.Rgba8888;
        var bpp = 4;
        var imageWidth = _imageWidth;
        var imageHeight = _imageHeight;
        var rowBytes = imageWidth * bpp;
        var totalBytes = imageHeight * rowBytes;
        var address = Marshal.AllocHGlobal(totalBytes);
        var rng = CreateRNG();

        await Task.Run(() =>
        {
            stopWatch.Start();

            unsafe
            {
                byte* p = (byte*)address.ToPointer();

                for (int y = 0; y < imageHeight; y++)
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        var index = (imageHeight - y - 1) * imageWidth * bpp + x * bpp;
                        var color = GetColorAt(x, y, rng);
                        p[index + 0] = color.R;
                        p[index + 1] = color.G;
                        p[index + 2] = color.B;
                        p[index + 3] = color.A;
                    }
                }
            }

            stopWatch.Stop();
        }, token);

        if (token.IsCancellationRequested)
        {
            Marshal.FreeHGlobal(address);
        }
        else
        {
            _bitmap = new Bitmap(
                pixelFormat,
                AlphaFormat.Unpremul,
                address,
                new PixelSize(_imageWidth, _imageHeight),
                new Vector(96, 96),
                rowBytes
            );
            ImageSource = _bitmap;

            Marshal.FreeHGlobal(address);

            InfoText = $"The image is generated in {stopWatch.ElapsedMilliseconds} ms.";
            IsGenerating = false;
        }
    }

    IRng CreateRNG()
    {
        switch (_rngType)
        {
            case RngType.SystemDefault:
                return new SystemRng(_rngSeed);
            default:
            case RngType.LCG:
                return new LCG(_rngSeed);
        }
    }

    Color GetColorAt(int x, int y, IRng rng)
    {
        var value = (byte)(0xff * rng.Random());
        return new Color(0xff, value, value, value);
    }

    interface IRng
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

    public class RngGeneratorLabel
    {
        public RngGeneratorLabel(string description, MainWindowViewModel.RngType rngType)
        {
            Description = description;
            RngType = rngType;
        }

        public string Description { get; }

        public MainWindowViewModel.RngType RngType { get; }
    }
}