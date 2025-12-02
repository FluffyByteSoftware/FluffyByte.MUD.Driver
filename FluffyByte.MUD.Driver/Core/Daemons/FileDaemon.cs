/*
 * (FileDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 9:27:27 PM
 * Created by - Seliris
 * Updated - Added ShutdownFlush and cache size threshold
 *-------------------------------------------------------------
 */
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

/// <summary>Provides functionalities to manage and monitor the operational status of
/// a file management daemon.</summary>
/// <remarks>The FileDaemon class is a static utility for keeping track of the daemon's lifecycle
/// state, including its uptime and latest start time. It can be queried to get the current
/// status of the daemon.</remarks>
public static class FileDaemon
{
    private static string Name => "filed";
    
    #region Life Cycle
    /// <summary>Provides static methods for managing the lifecycle of the FileDaemon, which is responsible
    /// for overseeing file-related operations within the system. This class handles startup requests,
    /// state reporting, and integration with the system daemon for coordinated functionality.</summary>
    /// <remarks>The FileDaemon class is designed to operate as a static utility for managing file-specific tasks
    /// in conjunction with the SystemDaemon. Its primary responsibilities include initialization, runtime
    /// state maintenance, and status reporting. Thread safety for static members is
    /// ensured where applicable.</remarks>
    static FileDaemon()
    {
        _state = DaemonStatus.Stopped;
    }
    
    private static CancellationTokenRegistration? _shutdownRegistration;
    private static DateTime _lastStartTime = DateTime.MaxValue;
    private static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime;
    private static DaemonStatus _state;
    

    /// <summary>Initiates a request to start the FileDaemon, transitioning its state to the appropriate
    /// status and preparing it for operational activities. This action is intended to enable
    /// the daemon to perform its designated tasks and maintain its lifecycle.</summary>
    public static void RequestStart()
    {
        if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            return;
        }

        if (_state is DaemonStatus.Starting or DaemonStatus.Running)
        {
            Log.Warn($"{Name}: RequestStart stopped due to state: {_state}.");
            return;
        }

        try
        {
            _state = DaemonStatus.Starting;
            _lastStartTime = DateTime.UtcNow;

            _shutdownRegistration = new CancellationTokenRegistration();
            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(RequestStop);
            
            _state = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"{Name}: RequestStart stopped due to shutdown.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStart()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Initiates a request to stop the FileDaemon, transitioning its state to the appropriate status
    /// and halting its operations. This action is intended for orderly shutdown and resource cleanup.</summary>
    private static void RequestStop()
    {
        if (_state is DaemonStatus.Stopping or DaemonStatus.Stopped)
        {
            Log.Warn($"{Name}: RequestStop stopped due to state: {_state}.");
            return;
        }

        try
        {
            _state = DaemonStatus.Stopping;

            _shutdownRegistration?.Dispose();
            _lastStartTime = DateTime.MaxValue;
            _state = DaemonStatus.Stopped;
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"{Name}: RequestStop stopped due to shutdown.");
            _state = DaemonStatus.Error;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception in RequestStop()", ex);
            _state = DaemonStatus.Error;
        }
    }

    /// <summary>Retrieves the current operational status of the FileDaemon, including its name,
    /// status, uptime, and the last start time.</summary>
    /// <returns>A string containing the name of the daemon, its current status,
    /// the duration for which it has been running, and the last time it was started.</returns>
    internal static string RequestStatus => $"{Name} -- {_state} -- {Uptime}";

    #endregion Life Cycle
    
    #region File Cacheing and Buffering

    private static readonly Dictionary<string, byte[]> FileCache = [];
    private static readonly Lock CacheLock = new Lock();

    private static readonly Dictionary<string, byte[]> WriteQueue = [];
    private static readonly Lock WriteLock = new Lock();
    
    /// <summary>
    /// Calculates the total size in bytes of all cached files.
    /// </summary>
    private static long GetCacheSize()
    {
        lock (CacheLock)
        {
            return FileCache.Values.Sum(data => data.LongLength);
        }
    }

    /// <summary>Reads the contents of a file at the specified path into a byte array. If the file has been
    /// previously read, the method attempts to retrieve the data from an in-memory cache to improve
    /// performance.</summary>
    /// <param name="filePath">The path of the file to be read. This should be a valid file path.</param>
    /// <returns>A byte array containing the contents of the file if the operation succeeds.
    /// Returns null if the file does not exist, the file path is invalid, or an error occurs during the read operation.</returns>
    public static byte[]? ReadFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        lock (CacheLock)
        {
            if (FileCache.TryGetValue(filePath, out var cached))
            {
                Log.Debug($"{Name}: Cache hit for {filePath}");
                return cached;
            }
        }

        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warn($"{Name}: File not found on disk: {filePath}");
                return null;
            }

            var fileData = File.ReadAllBytes(filePath);

            lock (CacheLock)
            {
                FileCache[filePath] = fileData;
            }

            Log.Debug($"{Name}: Loaded from disk and cached: {filePath}");

            return fileData;
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception during ReadFile: {ex}");
            return null;
        }
    }

    /// <summary>Writes the provided data to the specified file path and queues the write operation for further
    /// processing. The data is temporarily stored in an internal cache and added to the writing queue to ensure
    /// the operation completes in a consistent and controlled manner.</summary>
    /// <param name="filePath">The path of the file to which the data will be written. Must not be null
    /// or empty.</param>
    /// <param name="data">The byte array representing the data to write. Must not be null.</param>
    public static void WriteFile(string filePath, byte[]? data)
    {
        if (string.IsNullOrEmpty(filePath) || data == null)
        {
            return;
        }

        lock (CacheLock)
        {
            FileCache[filePath] = data;
        }

        lock (WriteLock)
        {
            // Queue this file for disk write on the next tick
            WriteQueue[filePath] = data;
        }

        Log.Debug($"{Name}: Queued write for {filePath}");
    }
    #endregion File Cacheing and Buffering
    
    #region Tick Operations

    /// <summary>Executes operations that need to occur periodically based on a tick count,
    /// handling tasks such as flushing the writing queue and logging errors.</summary>
    /// <param name="tickCount">The current tick count, representing the number of elapsed periods since the
    /// start of the daemon's operation.</param>
    /// <returns>A task representing the asynchronous operation, allowing the method to be awaited by
    /// callers.</returns>
    public static async Task Tick(long tickCount)
    {
        try
        {
            // Check if cache has exceeded maximum allowable size
            long cacheSize = GetCacheSize();
            if (cacheSize > Constellations.FlushThresholdBytes)
            {
                Log.Warn($"{Name}: Cache size ({cacheSize} bytes) exceeds threshold ({Constellations.FlushThresholdBytes} bytes). Force flushing.");
            }
            
            await FlushWriteQueue();
        }
        catch(Exception ex)
        {
            Log.Error($"{Name}: Exception during Tick({tickCount})", ex);
        }
    }

    /// <summary>
    /// Immediately flushes all pending writes to disk. Called during system shutdown.
    /// </summary>
    public static async Task ShutdownFlush()
    {
        try
        {
            Log.Info($"{Name}: ShutdownFlush() initiated.");
            await FlushWriteQueue();
            Log.Info($"{Name}: ShutdownFlush() completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: Exception during ShutdownFlush()", ex);
        }
    }

    private static async Task FlushWriteQueue()
    {
        Dictionary<string, byte[]> pendingWrites;

        lock (WriteLock)
        {
            if (WriteQueue.Count == 0)
                return;
            
            pendingWrites = new Dictionary<string, byte[]>(WriteQueue);
            WriteQueue.Clear();
        }

        foreach (var (filePath, data) in pendingWrites)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) &&
                    !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(filePath, data);

                Log.Debug($"{Name}: Flush to disk: {filePath}");
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                Log.Warn($"{Name}: FlushWriteQueue stopped due to shutdown.");
                Log.Error($"{Name}: FlushWriteQueue failed for {filePath}, with remaining " +
                          $"{data.Length} items.");
            }
            catch (Exception ex)
            {
                Log.Error($"{Name}: FlushWriteQueue failed for {filePath}; remaining: {data.Length}", ex);
            }
        }
    }
    #endregion Tick Operations
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/