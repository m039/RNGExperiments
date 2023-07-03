using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
        var vm = new MainWindowViewModel();
        vm.OnGenerationFinished += () => {
            imagePanel.Children.Remove(imageScrollViewer);
            imagePanel.Children.Add(imageScrollViewer);
        };
        DataContext = vm;
        rngSelectedBox.SelectedIndex = 0;
        vm.Ready();
    }
}