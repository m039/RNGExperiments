using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using Avalonia;
using System;

namespace RNGExperiments;

public class MainWindowViewModel {
    enum RngType {
        LCG, SystemDefault
    }

    Image _image;

    int _imageWidth = 300;

    int _imageHeight = 300;

    int _rngSeed = 0;

    RngType _rngType = RngType.LCG;

    Bitmap _bitmap = null;

    public MainWindowViewModel(Image image) {
        _image = image;
        UpdateImageSize();
    }

    public string ImageWidth {
        get => _imageWidth.ToString();
        set {
            if (int.TryParse(value, out var v)) {
                _imageWidth = v;
                UpdateImageSize();
            }
        }
    }

    public string ImageHeight {
        get => _imageHeight.ToString();
        set {
            if (int.TryParse(value, out var v)) {
                _imageHeight = v;
                UpdateImageSize();
            }
        }
    }

    public string RngSeed {
        get => _rngSeed.ToString();
        set {
            if (int.TryParse(value, out var v)) {
                _rngSeed = v;
                SetImage();
            }
        }
    }

    public object SelectedRngItem {
        set {            
            if (value is ContentControl control) {
                var text = control.GetValue(ContentControl.ContentProperty)?.ToString();
                _rngType = (RngType)Enum.Parse(typeof(RngType), text ?? RngType.LCG.ToString(), true);
                SetImage();
            }
        }
    }

    void UpdateImageSize() {
        _image.Width = _imageWidth;
        _image.Height = _imageHeight;
        SetImage();
    }

    void SetImage() {
        if (_bitmap != null) {
            _bitmap.Dispose();
        }

        var pixelFormat = PixelFormat.Rgba8888;
        var bpp = 4;
        var rowBytes = _imageWidth * bpp;
        var totalBytes = _imageHeight * rowBytes;
        var address = Marshal.AllocHGlobal(totalBytes);
        var rng = CreateRNG();

        unsafe {
            byte *p = (byte*)address.ToPointer();

            for (int y = 0; y < _imageHeight; y++) {
                for (int x = 0; x < _imageWidth; x++) {
                    var index = (_imageHeight - y - 1) * _imageWidth * bpp + x * bpp;
                    var color = GetColorAt(x, y, rng);
                    p[index + 0] = color.R;
                    p[index + 1] = color.G;
                    p[index + 2] = color.B;
                    p[index + 3] = color.A;
                }
            }
        }
        
        _image.Source = new Bitmap(
            pixelFormat, 
            AlphaFormat.Unpremul,
            address,
            new PixelSize(_imageWidth, _imageHeight),
            new Vector(96, 96), 
            rowBytes
            );

        Marshal.FreeHGlobal(address);
    }

    IRng CreateRNG() {
        switch (_rngType) {
            case RngType.SystemDefault:
                return new SystemRng(_rngSeed);
            default:
            case RngType.LCG:
                return new LCG(_rngSeed);
        }
    }

    Color GetColorAt(int x, int y, IRng rng) {
        var value = (byte)(0xff * rng.Random());
        return new Color(0xff, value, value, value);
    }

    interface IRng {
        double Random();
    }

    class SystemRng : IRng {
        Random _random;

        public SystemRng(int seed) {
            _random = new Random(seed);
        }

        public double Random()
        {
            return _random.NextDouble();
        }
    }

    class LCG : IRng {
        const int Modulus = 1 << 31;
        const int Multiplier = 1103515245;
        const int Increment = 12345;

        int _seed;

        public LCG(int seed) {
            _seed = seed;
        }

        public double Random() {
            _seed = (Multiplier * _seed + Increment) % Modulus;
            return _seed / (double)Modulus;
        }
    }
}