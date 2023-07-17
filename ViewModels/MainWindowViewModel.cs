using Avalonia.Media;
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
using System.Reactive;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;

namespace RNGExperiments;

public class MainWindowViewModel : ViewModelBase
{
    int _imageWidth = 3000;

    int _imageHeight = 3000;

    int _rngSeed = 0;

    Bitmap? _bitmap;

    IImage? _imageSource;

    string _infoText = string.Empty;

    bool _isReady;

    bool _isGenerating;

    CancellationTokenSource? _cancellationToken;

    RngGeneratorLabel? _selectedRngItem;

    ColorModeLabel? _colorModeItem;

    public MainWindowViewModel()
    {
        // Rng labels.
        var rngLabels = new List<RngGeneratorLabel>();

        foreach (RngType rngType in Enum.GetValues(typeof(RngType)))
        {
            var label = new RngGeneratorLabel(
                rngType.GetDescription(),
                rngType
            );

            _selectedRngItem ??= label;

            rngLabels.Add(label);
        }

        RngGeneratorLabels = rngLabels;

        // Color mode labels.
        var colorModeLabels = new List<ColorModeLabel>();
        _colorModeItem = new ColorModeLabel("Gray", true);
        colorModeLabels.Add(_colorModeItem);
        colorModeLabels.Add(new ColorModeLabel("Color", false));
        ColorModeLabels = colorModeLabels;

        // Commands.
        CheckWhenRepeats = ReactiveCommand.Create(() =>
        {
            return new CheckWhenRepeatsViewModel(
                _selectedRngItem!.RngType,
                _rngSeed
            );
        }, this.WhenAnyValue(x => x.SelectedRngItem,
            x => x != null && x.RngType.CanCheckWhenRepeats()));

        SaveImage = ReactiveCommand.Create(async () =>
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            var storageFile = await provider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save Image"
            });

            if (_bitmap != null && storageFile != null)
            {
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                void action()
                {
                    try
                    {
                        var fileStream = File.Create(
                            storageFile.Path.AbsolutePath,
                            1024 * 1024, 
                            FileOptions.Asynchronous
                            );
                        using var steam = new CancelableFileStream(fileStream, token);
                        _bitmap.Save(steam);
                    }
                    catch (OperationCanceledException)
                    {
                        File.Delete(storageFile.Path.AbsolutePath);
                    }

                    tokenSource.Dispose();
                }
                void cancel() { 
                    tokenSource.Cancel(); 
                }

                return new BackgroundOperationViewModel("Saving the Image", action, cancel);
            }

            return null;
        }, this.WhenAnyValue(x => x.ImageSource, (IImage? image) => image != null));
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

    public ReactiveCommand<Unit, CheckWhenRepeatsViewModel> CheckWhenRepeats { get; }

    public ReactiveCommand<Unit, Task<BackgroundOperationViewModel?>> SaveImage { get; }

    public RngGeneratorLabel? SelectedRngItem
    {
        get => _selectedRngItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRngItem, value);
            if (value != null)
            {
                SetImage();
            }
        }
    }

    public ColorModeLabel? ColorModeItem
    {
        get => _colorModeItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _colorModeItem, value);
            if (value != null)
            {
                SetImage();
            }
        }
    }

    public IEnumerable<RngGeneratorLabel> RngGeneratorLabels { get; }

    public IEnumerable<ColorModeLabel> ColorModeLabels { get; }

    public event System.Action? OnGenerationFinished;

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
        var rng = _selectedRngItem!.RngType.Create(_rngSeed);
        var isGray = _colorModeItem!.IsGray;

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
                        var color = GetColorAt(x, y, rng, isGray);
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
            OnGenerationFinished?.Invoke();
        }
    }

#pragma warning disable IDE0060
    static Color GetColorAt(int x, int y, IRng rng, bool isGray)
    {
        if (isGray)
        {
            var value = (byte)(0xff * rng.Random());
            return new Color(0xff, value, value, value);
        }
        else
        {
            var value = rng.RandomUInt();
            return new Color(
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)(value & 0xff)
            );
        }
    }
#pragma warning restore IDE0060

    public class ColorModeLabel
    {
        public ColorModeLabel(string description, bool isGray)
        {
            IsGray = isGray;
            Description = description;
        }

        public bool IsGray { get; }

        public string Description { get; }
    }

    public class RngGeneratorLabel
    {
        public RngGeneratorLabel(string description, RngType rngType)
        {
            Description = description;
            RngType = rngType;
        }

        public string Description { get; }

        public RngType RngType { get; }
    }
}
