/*
 * (Heartbeat.cs)
 *------------------------------------------------------------
 * Created - 11/23/2025 10:18:04 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Diagnostics;
using FluffyByte.MUD.Driver.Core.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Types.Heartbeats;

public sealed class Heartbeat(TimeSpan interval, Func<long, Task> onTickAsync) : IAsyncDisposable
{
    private PeriodicTimer? _timer;
    private readonly Func<long, Task> _onTickAsync = onTickAsync;

    private Task? _loopTask;

    private long _tickCount = 0;
    private TimeSpan _timeSinceLastTick = TimeSpan.MinValue;
    private readonly TimeSpan _interval = interval;

    public TimeSpan TimeSinceLastTick => _timeSinceLastTick;

    public long TickCount => Interlocked.Read(ref _tickCount);

    /// <summary>
    /// Must be called once. Initializes the timer and starts the loop.
    /// </summary>
    public ValueTask Start()
    {
        // Initialize timer before starting loop — eliminates race
        _timer = new PeriodicTimer(_interval);

        // Start loop after timer is valid
        _loopTask = Task.Run(LoopAsync);

        return ValueTask.CompletedTask;
    }

    public ValueTask Stop()
    {
        if (_timer is null)
            return ValueTask.CompletedTask;

        _timer.Dispose();

        return ValueTask.CompletedTask;
    }

    private async Task LoopAsync()
    {
        if(_timer == null)
        {
            Log.Error($"LoopAsync() called before Start()");
            return;
        }

        try
        {
            var sw = Stopwatch.StartNew();


            while (await _timer.WaitForNextTickAsync(SystemDaemon.GlobalShutdownToken).ConfigureAwait(false))
            {
                Interlocked.Exchange(ref _tickCount, _tickCount + 1);
                _timeSinceLastTick = sw.Elapsed;
                sw.Restart();

                await _onTickAsync(_tickCount).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Guaranteed final tick after cancellation
            var finalTick = Interlocked.Increment(ref _tickCount);

            try
            {
                await _onTickAsync(finalTick).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error($"Exception during final heartbeat tick: {ex}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_loopTask != null)
            {
                await _loopTask.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // expected
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during Heartbeat disposal: {ex}");
        }

        _timer?.Dispose();
    }
}

/*
*------------------------------------------------------------
* (Heartbeat.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
