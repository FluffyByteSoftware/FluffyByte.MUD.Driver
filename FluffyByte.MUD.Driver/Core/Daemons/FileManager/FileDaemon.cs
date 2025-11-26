/*
 * (FileDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 9:27:27 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Collections.Concurrent;
using System.IO.Enumeration;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;
using FluffyByte.MUD.Driver.Core.Types.Heartbeats;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons.FileManager;

/// <summary>
/// Represents the FileDaemon which is responsible for managing file-related operations for the system.
/// </summary>
public static class FileDaemon
{
    public static string Name => "filed";

    private static readonly Lock _dirtyLock = new();

    private static Heartbeat? _systemFastHeartbeat;
    private static Heartbeat? _systemSlowHeartbeat;
    private static Heartbeat? _gameHeartbeat;

    private const int FlushThresholdBytes = 30 * 1024 * 1024;

    public static DaemonStatus State { get; private set; } = DaemonStatus.Stopped;

    private static readonly ConcurrentDictionary<string, FileEntry> _cache = [];
    private static readonly ConcurrentDictionary<string, byte> _dirty = [];

    private static long _queuedBytes;

    // ---------------------------------------------------------
    // Public API
    // ---------------------------------------------------------

    public static async Task RequestStart()
    {
        _systemFastHeartbeat = new(TimeSpan.FromMilliseconds(5000), SystemFastTick);
        _systemSlowHeartbeat = new(TimeSpan.FromMilliseconds(10000), SystemSlowTick);
        _gameHeartbeat = new(TimeSpan.FromMilliseconds(30000), GameTick);

        try
        {
            await _systemFastHeartbeat.Start();
            await _systemSlowHeartbeat.Start();
            await _gameHeartbeat.Start();
        }
        catch (OperationCanceledException)
        {
            Log.Debug("RequestStart canceled due to shutdown.");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static async Task RequestStop()
    {
        if (_systemFastHeartbeat is null ||
            _systemSlowHeartbeat is null ||
            _gameHeartbeat is null)
        {
            Log.Warn("A heartbeat timer was null; cannot stop completely.");
            return;
        }

        await FlushAll();

        await _systemFastHeartbeat.Stop();
        await _systemSlowHeartbeat.Stop();
        await _gameHeartbeat.Stop();

        State = DaemonStatus.Stopped;
    }

    public static async Task<byte[]?> ReadFile(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
            return cached.Content;

        if (!File.Exists(path))
        {
            Log.Warn($"ReadFile failed: not found: {path}");
            return null;
        }

        byte[] bytes = await File.ReadAllBytesAsync(path);

        // Cache as clean (not dirty)
        _cache[path] = new FileEntry(path, bytes);

        return bytes;
    }

    public static void WriteFile(string path, byte[] newContent)
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"WriteFile blocked during shutdown: {path}");
            return;
        }

        SetCacheEntry(path, newContent, dirty: true);
    }

    // ---------------------------------------------------------
    // Heartbeat ticks
    // ---------------------------------------------------------

    private static Task SystemFastTick(long _) => CheckFlush();
    private static Task SystemSlowTick(long _) => CheckFlush();
    private static Task GameTick(long _) => CheckFlush();

    // ---------------------------------------------------------
    // Internals
    // ---------------------------------------------------------

    private static void SetCacheEntry(string path, byte[] content, bool dirty)
    {
        var entry = _cache.AddOrUpdate(
            path,
            p => new FileEntry(p, content),
            (_, existing) =>
            {
                existing.Update(content);
                return existing;
            });

        if (dirty)
        {
            _dirty[path] = 0;
            Interlocked.Exchange(ref _queuedBytes, CalculateDirtyBytes());
        }
    }

    private static long CalculateDirtyBytes()
    {
        long total = 0;

        foreach (var p in _dirty.Keys)
        {
            if (_cache.TryGetValue(p, out var e))
                total += e.SizeBytes;
        }

        return total;
    }

    private static async Task CheckFlush()
    {
        if (_dirty.IsEmpty)
            return;

        if (Interlocked.Read(ref _queuedBytes) >= FlushThresholdBytes)
        {
            await FlushAll();
            return;
        }

        await FlushAll();
    }

    private static async Task FlushAll()
    {
        if (_dirty.IsEmpty)
            return;

        foreach (var path in _dirty.Keys)
        {
            if (!_cache.TryGetValue(path, out var entry))
                continue;

            byte[] data = entry.Content;

            try
            {
                await File.WriteAllBytesAsync(path, data);
            }
            catch (Exception ex)
            {
                Log.Error($"Flush failed for {path}: {ex.Message}");
            }
        }

        lock (_dirtyLock)
        {
            _dirty.Clear();
        }

        Interlocked.Exchange(ref _queuedBytes, 0);
    }
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/