using FluffyByte.MUD.Driver.Core.Types.Heartbeats;

namespace FluffyByte.MUD.Test;

public class HeartbeatTests
{
    #region Lifecycle Tests

    [Fact]
    public async Task Start_InitializesTimer_IsRunningTrue()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        await heartbeat.Start();

        Assert.True(heartbeat.IsRunning);

        await heartbeat.Stop();
        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task Stop_AfterStart_IsRunningFalse()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        await heartbeat.Start();
        await heartbeat.Stop();

        Assert.False(heartbeat.IsRunning);

        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task Stop_BeforeStart_DoesNotThrow()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        // Should not throw
        await heartbeat.Stop();

        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task Stop_CalledMultipleTimes_DoesNotThrow()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        await heartbeat.Start();
        await heartbeat.Stop();
        await heartbeat.Stop();
        await heartbeat.Stop();

        // No exception = pass
        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithoutStart_DoesNotThrow()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        // Should not throw
        await heartbeat.DisposeAsync();
    }

    #endregion

    #region Tick Counting Tests

    [Fact]
    public async Task TickCount_InitiallyZero()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(100), _ => Task.CompletedTask);

        Assert.Equal(0, heartbeat.TickCount);

        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task TickCount_IncrementsOnEachTick()
    {
        var ticksReceived = new List<long>();
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(50),
            tick =>
            {
                ticksReceived.Add(tick);
                return Task.CompletedTask;
            });

        await heartbeat.Start();

        // Wait for a few ticks
        await Task.Delay(180);

        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Should have received at least 2-3 ticks
        Assert.True(ticksReceived.Count >= 2, $"Expected at least 2 ticks, got {ticksReceived.Count}");

        // Ticks should be sequential starting from 1
        for (int i = 0; i < ticksReceived.Count; i++)
        {
            Assert.Equal(i + 1, ticksReceived[i]);
        }
    }

    [Fact]
    public async Task TickCount_MatchesCallbackInvocations()
    {
        int callbackCount = 0;
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(50),
            _ =>
            {
                Interlocked.Increment(ref callbackCount);
                return Task.CompletedTask;
            });

        await heartbeat.Start();
        await Task.Delay(180);
        await heartbeat.Stop();

        // Allow loop to finish
        await heartbeat.DisposeAsync();

        // TickCount should match callback invocations (within 1 for final tick)
        long finalTickCount = heartbeat.TickCount;
        Assert.True(
            Math.Abs(finalTickCount - callbackCount) <= 1,
            $"TickCount ({finalTickCount}) should roughly match callback count ({callbackCount})");
    }

    #endregion

    #region Callback Execution Tests

    [Fact]
    public async Task Callback_ReceivesIncrementingTickNumbers()
    {
        var ticks = new List<long>();
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(30),
            tick =>
            {
                ticks.Add(tick);
                return Task.CompletedTask;
            });

        await heartbeat.Start();
        await Task.Delay(120);
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Verify sequential ordering
        for (int i = 1; i < ticks.Count; i++)
        {
            Assert.Equal(ticks[i - 1] + 1, ticks[i]);
        }
    }

    [Fact]
    public async Task Callback_ExceptionDoesNotStopHeartbeat()
    {
        int tickCount = 0;
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(50),
            tick =>
            {
                Interlocked.Increment(ref tickCount);

                if (tick == 2)
                    throw new InvalidOperationException("Simulated failure");

                return Task.CompletedTask;
            });

        await heartbeat.Start();
        await Task.Delay(201);
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Should have continued past the exception
        Assert.True(tickCount >= 3, $"Expected at least 3 ticks despite exception, got {tickCount}");
    }

    [Fact]
    public async Task Callback_AsyncOperationsComplete()
    {
        var completedTicks = new List<long>();
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(50),
            async tick =>
            {
                // Simulate async work
                await Task.Delay(10);
                completedTicks.Add(tick);
            });

        await heartbeat.Start();
        await Task.Delay(180);
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        Assert.True(completedTicks.Count >= 2, $"Expected completed async ticks, got {completedTicks.Count}");
    }

    #endregion

    #region Timing Tests

    [Fact]
    public async Task TimeSinceLastTick_UpdatesAfterTick()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(50), _ => Task.CompletedTask);

        // Before start, should be MinValue
        Assert.Equal(TimeSpan.MinValue, heartbeat.TimeSinceLastTick);

        await heartbeat.Start();
        await Task.Delay(120); // Wait for at least one tick
        await heartbeat.Stop();

        // After ticks, should be a reasonable positive value
        Assert.NotEqual(TimeSpan.MinValue, heartbeat.TimeSinceLastTick);
        Assert.True(heartbeat.TimeSinceLastTick > TimeSpan.Zero);

        await heartbeat.DisposeAsync();
    }

    [Fact]
    public async Task Interval_TicksOccurAtApproximateInterval()
    {
        var tickTimes = new List<DateTime>();
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(100),
            _ =>
            {
                tickTimes.Add(DateTime.UtcNow);
                return Task.CompletedTask;
            });

        await heartbeat.Start();
        await Task.Delay(350); // Should get ~3 ticks
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Check intervals between ticks
        for (int i = 1; i < tickTimes.Count; i++)
        {
            var interval = tickTimes[i] - tickTimes[i - 1];

            // Allow 50ms tolerance for scheduling variance
            Assert.True(
                interval.TotalMilliseconds >= 50 && interval.TotalMilliseconds <= 150,
                $"Interval {interval.TotalMilliseconds}ms outside expected range [50-150ms]");
        }
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task TickCount_ThreadSafeReads()
    {
        var heartbeat = new Heartbeat(TimeSpan.FromMilliseconds(20), _ => Task.CompletedTask);

        await heartbeat.Start();

        // Hammer TickCount from multiple threads
        var readTasks = Enumerable.Range(0, 10)
            .Select(x => Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    _ = (int)heartbeat.TickCount; // Should never throw
                }
            }))
            .ToArray();

        await Task.WhenAll(readTasks);

        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // No exceptions = pass
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task VeryShortInterval_HandlesRapidTicks()
    {
        int tickCount = 0;
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(10),
            _ =>
            {
                Interlocked.Increment(ref tickCount);
                return Task.CompletedTask;
            });

        await heartbeat.Start();
        await Task.Delay(100);
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Should have gotten several rapid ticks
        Assert.True(tickCount >= 5, $"Expected at least 5 rapid ticks, got {tickCount}");
    }

    [Fact]
    public async Task SlowCallback_DoesNotAccumulateBacklog()
    {
        var tickTimes = new List<DateTime>();
        var heartbeat = new Heartbeat(
            TimeSpan.FromMilliseconds(50),
            async _ =>
            {
                tickTimes.Add(DateTime.UtcNow);
                await Task.Delay(30); // Callback takes 30ms, interval is 50ms
            });

        await heartbeat.Start();
        await Task.Delay(250);
        await heartbeat.Stop();
        await heartbeat.DisposeAsync();

        // Ticks should still be reasonably spaced, not bunched up
        Assert.True(tickTimes.Count >= 2 && tickTimes.Count <= 6,
            $"Got unexpected tick count: {tickTimes.Count}");
    }

    #endregion
}