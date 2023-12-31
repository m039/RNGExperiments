using System.Diagnostics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace RNGExperiments;

public class CheckWhenRepeatsViewModel : ViewModelBase
{
    readonly RngType _rngType;

    readonly int _rngSeed;

    string _infoText = string.Empty;

    readonly CancellationTokenSource _cancellationToken = new();

    bool _isRunning;

    long _iterations;

    public CheckWhenRepeatsViewModel(RngType rngType, int rngSeed)
    {
        _rngType = rngType;
        _rngSeed = rngSeed;
        _isRunning = true;
        InfoText = "...";
        Cancel = ReactiveCommand.Create(() =>
        {
            _cancellationToken.Cancel();
        });

        if (Design.IsDesignMode)
            return;

        CheckWhenRepeats();
    }

    public string InfoText
    {
        get => _infoText;
        set => this.RaiseAndSetIfChanged(ref _infoText, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    async void CheckWhenRepeats()
    {
        var token = _cancellationToken.Token;
        var stopWatch = new Stopwatch();
        var rng = _rngType.Create(_rngSeed);

        await Task.Run(() =>
        {
            stopWatch.Start();
            var previousTime = stopWatch.ElapsedMilliseconds;
            var targetTime = 0L;

            while (true)
            {
                var currentTime = stopWatch.ElapsedMilliseconds;
                var deltaTime = currentTime - previousTime;
                previousTime = currentTime;
                targetTime += deltaTime;

                if (targetTime > 1000)
                {
                    targetTime = 0;
                    InfoText = "Elapsed time: " + stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
                }

                rng.Random();

                if (rng.IsStartedRepeat()) {
                    break;
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                _iterations++;
            }

            stopWatch.Stop();
        });

        IsRunning = false;
        if (!token.IsCancellationRequested)
        {
            InfoText = $"{_rngType.GetDescription()} starts repeat itself in {stopWatch.Elapsed:hh\\:mm\\:ss} with {_iterations} iterations";
        }
    }
}
