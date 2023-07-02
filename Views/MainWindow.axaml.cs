using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RNGExperiments;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        rngSelectedBox.SelectedIndex = 0;
        DataContext = new MainWindowViewModel();
    }
}