using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace RNGExperiments;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        this.WhenActivated(action =>
        {
            ViewModel!.OnGenerationFinished += () =>
            {
                imagePanel.Children.Remove(imageScrollViewer);
                imagePanel.Children.Add(imageScrollViewer);
            };
            ViewModel!.Ready();
            action(ViewModel!.CheckWhenRepeats.Subscribe(ShowDialog));
        });
    }

    async void ShowDialog(CheckWhenRepeatsViewModel vm) {
        var dialog = new CheckWhenRepeatsWindow {
            DataContext = vm
        };
        await dialog.ShowDialog(this);
    }
}
