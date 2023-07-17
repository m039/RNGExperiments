using System;
using System.Threading.Tasks;
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
            action(ViewModel!.CheckWhenRepeats.Subscribe(ShowCheckWhenRepeatsDialog));
            action(ViewModel!.SaveImage.Subscribe(ShowBackgroindOperationDialog));
        });
    }

    async void ShowCheckWhenRepeatsDialog(CheckWhenRepeatsViewModel argument) {
        var dialog = new CheckWhenRepeatsWindow {
            DataContext = argument
        };
        await dialog.ShowDialog(this);
    }

    async void ShowBackgroindOperationDialog(Task<BackgroundOperationViewModel?> argument) {
        var vm = await argument;
        if (vm == null)
            return;

        var dialog = new BackgroundOperationWindow {
            DataContext = vm,
            Title = vm.Tilte
        };
        await dialog.ShowDialog(this);
    }
}
