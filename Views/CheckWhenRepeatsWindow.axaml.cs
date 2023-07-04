using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace RNGExperiments;

public partial class CheckWhenRepeatsWindow : ReactiveWindow<CheckWhenRepeatsViewModel>
{
    public CheckWhenRepeatsWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        this.WhenActivated(action => action(ViewModel!.Cancel.Subscribe(x => Close(x))));

        Closing += (args, window) => ViewModel!.Cancel.Execute().Subscribe();
    }
}
