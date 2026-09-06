namespace EndfieldCharge.Services;

/// <summary>Single sequential polling loop; skips unknown states and confirms transitions.</summary>
public sealed class PowerWatcher : IDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _loop;
    public event EventHandler<bool>? PowerSourceChanged;
    // Reserved for a later power-profiles-daemon implementation.
    public event EventHandler<bool>? PowerSavingChanged { add { } remove { } }
    public bool IsRunning => _loop is { IsCompleted: false };

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        _loop = RunAsync(_cts.Token);
    }

    private async Task RunAsync(CancellationToken token)
    {
        bool? previous = BatteryService.GetSnapshot()?.AcOnline;
        try
        {
            while (true)
            {
                await Task.Delay(2000, token);
                var current = BatteryService.GetSnapshot()?.AcOnline;
                if (!current.HasValue) continue;
                if (previous.HasValue && current != previous)
                {
                    await Task.Delay(400, token);
                    if (BatteryService.GetSnapshot()?.AcOnline != current) continue;
                    token.ThrowIfCancellationRequested();
                    PowerSourceChanged?.Invoke(this, current.Value);
                }
                previous = current;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex) { Logger.Error(ex); }
    }

    public void Stop() => _cts?.Cancel();
    public void Dispose() { Stop(); _cts?.Dispose(); }
}
