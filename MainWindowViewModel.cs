using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using Avalonia;

namespace RNGExperiments;

public class MainWindowViewModel {

    Image _image;

    int _imageWidth = 300;

    int _imageHeight = 300;

    Color _imageColor = Colors.Black;

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

    public object SelectedColorItem {
        set {            
            if (value is ContentControl control) {
                var colorText = control.GetValue(ContentControl.ContentProperty)?.ToString();
                _imageColor = Color.Parse(colorText ?? "Black");
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

        unsafe {
            byte *p = (byte*)address.ToPointer();

            for (int y = 0; y < _imageHeight; y++) {
                for (int x = 0; x < _imageWidth; x++) {
                    var index = (_imageHeight - y - 1) * _imageWidth * bpp + x * bpp;
                    var color = GetColorAt(x, y);
                    p[index + 0] = color.R;
                    p[index + 1] = color.G;
                    p[index + 2] = color.B;
                    p[index + 3] = color.A;
                }
            }
        }
        
        _image.Source = new Bitmap(
            pixelFormat, 
            AlphaFormat.Opaque,
            address,
            new PixelSize(_imageWidth, _imageHeight),
            new Vector(96, 96), 
            rowBytes
            );

        Marshal.FreeHGlobal(address);
    }

    Color GetColorAt(int x, int y) {
        return _imageColor;
    }
}