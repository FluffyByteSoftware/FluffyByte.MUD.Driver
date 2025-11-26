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

/// <summary>
/// Provides a periodic asynchronous heartbeat mechanism that invokes a user-supplied callback at a specified interval.
/// </summary>
/// <remarks>Use this class to trigger periodic actions in the background, such as health checks or status
/// updates. The heartbeat runs asynchronously and is suitable for scenarios where regular, non-blocking execution is
/// required. Thread safety is ensured for tick counting and timer management. Dispose the instance when no longer
/// needed to release resources and stop background operations.</remarks>
/// <param name="interval">The time interval between consecutive heartbeat ticks. Must be a positive duration.</param>
/// <param name="onTickAsync">An asynchronous callback that is invoked on each tick, receiving the current tick count as its argument.</param>
public sealed class Heartbeat(TimeSpan interval, Func<long, Task> onTickAsync) : IAsyncDisposable
{
    private PeriodicTimer? _timer;
    private readonly Func<long, Task> _onTickAsync = onTickAsync;
    private Task? _loopTask;
    private long _tickCount = 0;
    private TimeSpan _timeSinceLastTick = TimeSpan.MinValue;
    private readonly TimeSpan _interval = interval;

    /// <summary>
    /// Gets the elapsed time since the last tick event was processed.
    /// </summary>
    public TimeSpan TimeSinceLastTick => _timeSinceLastTick;

    /// <summary>
    /// Gets the current tick count value in a thread-safe manner.
    /// </summary>
    public long TickCount => Interlocked.Read(ref _tickCount);
    /// <summary>
    /// Gets a value indicating whether the timer is currently active.
    /// </summary>
    public bool IsRunning => _timer != null;

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

    /// <summary>
    /// Stops the timer and releases associated resources.
    /// </summary>
    /// <remarks>Calling this method multiple times has no effect if the timer is already stopped or has not
    /// been started.</remarks>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous stop operation. The returned task is completed when
    /// the timer has been stopped.</returns>
    public Task Stop()
    {
        if (_timer is null)
            return Task.CompletedTask;

        if(!SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Debug($"GlobalShutdownToken is not cancelled, but a stop request to this heartbeat has been made.");
        }
        
        _timer.Dispose();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously releases resources used by the instance.
    /// </summary>
    /// <remarks>Call this method to clean up resources when the instance is no longer needed. This method
    /// should be awaited to ensure all background operations have completed before disposal.</remarks>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
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

    /// <summary>
    /// Iterates a loop in a threadsafe manner.
    /// </summary>
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
        catch(Exception ex)
        {
            Log.Error($"Exception in LoopAsync", ex);
        }
    }
}

/*
*------------------------------------------------------------
* (Heartbeat.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
