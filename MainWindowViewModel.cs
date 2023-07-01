using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace RNGExperiments;

public class MainWindowViewModel {

    Rectangle _rectangle;

    public MainWindowViewModel(Rectangle rectangle) {
        _rectangle = rectangle;
    }

    object? _selectedColorItem;

    public object? SelectedColorItem {
        get => _selectedColorItem;
        set {
            _selectedColorItem = value;
            
            if (value is ContentControl control) {
                var colorText = control.GetValue(ContentControl.ContentProperty)?.ToString();
                _rectangle.Fill = new SolidColorBrush(Color.Parse(colorText ?? "Black"));
            }
        }
    }
}