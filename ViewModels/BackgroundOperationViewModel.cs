using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace RNGExperiments;

public class BackgroundOperationViewModel : ViewModelBase
{
    readonly string _title;

    System.Action? _cancel;

    public BackgroundOperationViewModel(
        string title,
        System.Action operation,
        System.Action cancel)
    {
        _title = title;
        _cancel = cancel;

        Cancel = ReactiveCommand.Create(() => {
            _cancel?.Invoke();
        });

        Task.Run(async () => {
            await Task.Run(operation);
            _cancel = null;
            Cancel.Execute().Subscribe();
        });
    }

    public string Tilte => _title;

    public ReactiveCommand<Unit, Unit> Cancel { get; }
}