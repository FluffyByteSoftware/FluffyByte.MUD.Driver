/*
 * (FileDaemon.cs)
 *------------------------------------------------------------
 * Created - 11/24/2025 9:27:27 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Collections.Concurrent;
using FluffyByte.MUD.Driver.Core.Types.Daemons;
using FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;
using FluffyByte.MUD.Driver.Core.Types.Heartbeats;
using FluffyByte.MUD.Driver.FluffyTools;

namespace FluffyByte.MUD.Driver.Core.Daemons;

/// <summary>
/// Provides static methods and properties for managing file operations and background heartbeat processing within the
/// application daemon. Supports asynchronous start and stop operations, file reading and writing with caching, and
/// prioritization of file flush queues.
/// </summary>
/// <remarks>FileDaemon is designed to operate as a singleton service, coordinating periodic background tasks and
/// file persistence. It maintains internal caches and flush queues to optimize file I/O and ensure data consistency.
/// All public methods are thread-safe and intended to be called from application-level code. The daemon's state
/// transitions through defined statuses to reflect its operational lifecycle.</remarks>
public static class FileDaemon
{
    /// <summary>
    /// Name of this Daemon (filed)
    /// </summary>
    public static string Name => "filed";
    
    private static CancellationTokenRegistration _shutdownRegistration;
    
    private static Heartbeat? _systemFastHeartbeat;
    private static Heartbeat? _systemLongHeartbeat;
    private static Heartbeat? _gameHeartbeat;

    private static Cache? _cacheFast;
    private static Cache? _cacheSlow;
    private static Cache? _cacheGame;
    
    /// <summary>
    /// Gets the current status of the filedaemon process.
    /// </summary>
    /// <remarks>The value reflects the most recent state transition of the daemon. This property is
    /// thread-safe and can be accessed from any thread.</remarks>
    public static DaemonStatus State { get; private set; } = DaemonStatus.Stopped;

    /// <summary>
    /// Stores the last starttime of the FileDaemon.
    /// </summary>
    private static DateTime? _lastStartTime;
    
    /// <summary>
    /// Gets the duration for which the daemon has been running since its last start.
    /// </summary>
    /// <remarks>If the daemon is not running or has never been started, the value
    /// is <see cref="TimeSpan.Zero"/>.</remarks>
    public static TimeSpan Uptime => DateTime.UtcNow - _lastStartTime ?? TimeSpan.Zero;

    #region Constructor
    static FileDaemon() { }
    #endregion
    
    #region Public API - Daemon Lifecycle
    /// <summary>
    /// Initiates the startup sequence for the daemon asynchronously, transitioning its state to running if successful.
    /// </summary>
    /// <remarks>If the startup process is canceled or encounters an error, the daemon state is designated as stopped
    /// or error, respectively. This method should be called before performing operations that require the daemon to be
    /// running.</remarks>
    /// <returns>A task that represents the asynchronous operation of starting the daemon.</returns>
    public static async Task RequestStart()
    {
        Log.Debug($"{Name}: RequestStart called.");
        
        if (State != DaemonStatus.Stopped && State != DaemonStatus.Stopping && State != DaemonStatus.Error)
            return;
        
        State = DaemonStatus.Starting;

        try
        {
            // Initialize caches
            _cacheFast = new Cache();
            _cacheSlow = new Cache();
            _cacheGame = new Cache();

            _systemFastHeartbeat = new Heartbeat(TimeSpan.FromSeconds(5), SystemFastTick);
            _systemLongHeartbeat = new Heartbeat(TimeSpan.FromMinutes(1), SystemSlowTick);
            _gameHeartbeat = new Heartbeat(TimeSpan.FromSeconds(30), GameTick);

            await _systemFastHeartbeat.Start();
            await _systemLongHeartbeat.Start();
            await _gameHeartbeat.Start();

            _lastStartTime = DateTime.UtcNow;

            Log.Debug($"{Name}: About to register shutdown callback...");

            _shutdownRegistration = SystemDaemon.GlobalShutdownToken.Register(OnShutdownInitiated);

            Log.Debug($"{Name}: Shutdown callback registered.  Token.CanbeCancelled: " +
                      $"{SystemDaemon.GlobalShutdownToken.CanBeCanceled}");

            State = DaemonStatus.Running;
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"RequestStart stopped due to shutdown.");
            State = DaemonStatus.Stopped;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception in RequestStart of {Name}", ex);
            State = DaemonStatus.Error;
        }
        finally
        {
            State = DaemonStatus.Running;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Initiates a graceful shutdown of the daemon by stopping all heartbeat timers asynchronously.
    /// </summary>
    /// <remarks>If an error occurs while stopping the heartbeat timers, the daemon state is set to error and
    /// the operation is aborted. The method should be called when a controlled stop of the daemon is
    /// required.</remarks>
    /// <returns>A task that represents the asynchronous stop operation. The task completes when all
    /// heartbeat timers have been
    /// stopped.</returns>
    /// <exception cref="NullReferenceException">Thrown if any of the required heartbeat timers are null.</exception>
    private static async Task RequestStop()
    {
        Log.Debug($"{Name}: RequestStop called.");

        if (State != DaemonStatus.Running && State != DaemonStatus.Starting)
        {
            Log.Warn($"FileDaemon was requested to stop but it is in state: {State}. Omitting.");
            return;
        }

        State = DaemonStatus.Stopping;

        if (_systemFastHeartbeat == null || _systemLongHeartbeat == null || _gameHeartbeat == null)
        {
            State = DaemonStatus.Error;
            throw new NullReferenceException("A heartbeat timer was null in RequestStop()");
        }

        try
        {
            // Flush everything before stopping
            await FlushQueue.FlushAll();

            await _systemFastHeartbeat.Stop();
            await _gameHeartbeat.Stop();
            await _systemLongHeartbeat.Stop();
            await _shutdownRegistration.DisposeAsync();
            State = DaemonStatus.Stopped;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            State = DaemonStatus.Error;
        }
    }

    private static async void OnShutdownInitiated()
    {
        try
        {
            Log.Info($"{Name}: Shutdown initated, flushing cache...");

            try
            {
                await RequestStop();
                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
    #endregion

    #region Heartbeats (Ticks)
    private static Task SystemFastTick(long _)
    {
        _cacheFast?.PruneStaleEntries((TimeSpan.FromMinutes(30)));
        
        return FlushQueue.CheckFlush(FilePriority.SystemFast);
    }

    /// <summary>
    /// The slow tick for system files
    /// </summary>
    /// <param name="_">The long value for the current tick count.</param>
    /// <returns>A FlushQueue</returns>
    private static Task SystemSlowTick(long _)
    {
        _cacheSlow?.PruneStaleEntries(TimeSpan.FromMinutes(30)); // Remove items unused at 30 minutes

        return FlushQueue.CheckFlush(FilePriority.SystemSlow);
    }

    private static Task GameTick(long _)
    {
        _cacheGame?.PruneStaleEntries((TimeSpan.FromMinutes(30)));
        
        return FlushQueue.CheckFlush(FilePriority.Game);
    }
    #endregion

    #region IO Component (IO Class)
    /// <summary>
    /// Provides static methods for reading, writing, and managing file data with caching and prioritization support.
    /// </summary>
    /// <remarks>The IO class offers asynchronous file operations that use an internal cache to optimize
    /// access and manage write priorities. All methods are thread-safe and intended for use in scenarios where file
    /// access performance and prioritization are important. File writes may be deferred and queued based on system
    /// state and priority. If a global shutdown is in progress, write operations are ignored to ensure system
    /// stability.</remarks>
    public static class InputOutput
    {
        /// <summary>
        /// Asynchronously reads the contents of the specified file and returns a byte array containing the data.
        /// </summary>
        /// <param name="path">Path to the file to read</param>
        /// <param name="priority">Which cache or queue priority to load this to</param>
        /// <returns>A byte array of file contents.</returns>
        public static async Task<byte[]?> Read(string path, FilePriority priority = FilePriority.Game)
        {
            if (_cacheFast == null || _cacheSlow == null || _cacheGame == null)
            {
                Log.Error(
                    $"One of the caches was null in Read!",
                    new NullReferenceException($"Null reference for a cache.")
                );
                return null;
            }

            // Check cache first based on priority
            var targetCache = priority switch
            {
                FilePriority.SystemFast => _cacheFast,
                FilePriority.SystemSlow => _cacheSlow,
                _=> _cacheGame
            };

            // Return from cache if available
            if (targetCache.TryGetValue(path, out FileEntry? cached))
                return cached?.Content;

            // Not in cache, read from disk
            if (!File.Exists(path))
            {
                Log.Warn($"Read failed: not found: {path}");
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(path, SystemDaemon.GlobalShutdownToken);

            // Cache it for next time (not dirty since we just read it)
            targetCache.SetEntry(path, bytes, dirty: false, priority);

            return bytes;
        }

        /// <summary>
        /// Asynchronously writes the specified data to the given path, using the provided file priority. </summary>
        /// <remarks>If a global shutdown is in progress, the write operation will be rejected and no data
        /// will be written.</remarks>
        /// <param name="path">The path where the data will be written. This should be a valid file
        /// system path.</param>
        /// <param name="data">The byte array containing the data to write to the specified path.
        /// Cannot be null.</param>
        /// <param name="priority">The priority level to assign to the write operation.
        /// Defaults to <see cref="FilePriority.Game"/> if not specified.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static void Write
        (string path,
            byte[] data,
            FilePriority priority = FilePriority.Game)
        {
            if (SystemDaemon.GlobalShutdownToken.IsCancellationRequested)
            {
                Log.Warn($"{path} requested to write, but we are currently shutting down. Rejected.");
                return;
            }

            if (_cacheFast == null || _cacheSlow == null || _cacheGame == null)
            {
                Log.Error($"One or more caches are null in Write request!");
                return;
            }

            try
            {
                switch (priority)
                {
                    case FilePriority.SystemFast:
                        _cacheFast.SetEntry(path, data, dirty: true, priority);
                        break;
                    case FilePriority.SystemSlow:
                        _cacheSlow.SetEntry(path, data, dirty: true, priority);
                        break;
                    case FilePriority.Game:
                        _cacheGame.SetEntry(path, data, dirty: true, priority);
                        break;
                    default:
                        Log.Error("Unknown priority in Write");
                        break;
                }   
            }
            catch (OperationCanceledException)
            {
                Log.Warn(
                    $"OperationCanceledException during a Write - CHECK TO SEE IF {path} is updated!"
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                State = DaemonStatus.Error;
            }
        }

        /// <summary>
        /// Calculates the total number of bytes currently pending to be flushed in the queue.
        /// </summary>
        /// <returns>The number of bytes that have not yet been flushed. Returns 0 if there are no pending
        /// bytes.</returns>
        public static long SizeUp()
        {
            return FlushQueue.CalculateDirtyBytes();
        }

        /// <summary>
        /// Returns a formatted string listing the names of files that are currently queued to be written.
        /// </summary>
        /// <remarks>The returned string lists all file names present in the dirty queues.</remarks>
        /// <returns>A comma-separated string containing the names of files waiting to be written. If no files
        /// are queued, returns a message indicating no files are waiting.</returns>
        public static string FilesWaitingToWrite()
        {
            if (_cacheFast == null || _cacheGame == null || _cacheSlow == null)
                throw new NullReferenceException(
                    "One or more caches were null in FilesWaitingToWrite"
                );

            List<string> allDirtyFiles = [];

            // Get dirty files from each queue
            allDirtyFiles.AddRange(FlushQueue.GetDirtyPaths(FilePriority.SystemFast));
            allDirtyFiles.AddRange(FlushQueue.GetDirtyPaths(FilePriority.SystemSlow));
            allDirtyFiles.AddRange(FlushQueue.GetDirtyPaths(FilePriority.Game));

            return allDirtyFiles.Count == 0 ? "No files waiting to write." :
                allDirtyFiles.ToCommaList();
        }
    }
    #endregion

    #region Cache Component
    /// <summary>
    /// Provides a thread-safe in-memory cache for storing and retrieving file entries by path.
    /// </summary>
    /// <remarks>The cache uses a concurrent dictionary to ensure safe access from multiple threads. Entries
    /// can be marked as dirty to indicate that they require flushing. This class is intended for internal use and is
    /// not thread-affinitive; callers may access its members from any thread.</remarks>
    private class Cache
    {
        /// <summary>
        /// Provides a thread-safe collection that maps file names to their corresponding file entries.
        /// </summary>
        private readonly ConcurrentDictionary<string, FileEntry> _entries = new();

        /// <summary>
        /// Attempts to retrieve the file entry associated with the specified path.
        /// </summary>
        /// <param name="path">The path of the file to locate. Cannot be null.</param>
        /// <param name="entry">When this method returns, contains the <see cref="FileEntry"/> associated with
        /// the specified path, if found; otherwise, <see langword="null"/>.</param>
        /// <returns>true if the file entry was found for the specified path; otherwise, false.</returns>
        internal bool TryGetValue(string path, out FileEntry? entry)
        {
            return _entries.TryGetValue(path, out entry);
        }

        internal void PruneStaleEntries(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;

            foreach (var (path, entry) in _entries)
            {
                // 1. If the file has been accessed recently, skip it.
                if (entry.LastAccess > cutoff)
                    continue;

                // 2. If the file is currently waiting to be written to disk.
                // DO NOT remove it, or the FlushQueue will fail.
                if (FlushQueue.IsDirty(path))
                    continue;

                // 3. Safe to remove
                _entries.TryRemove(path, out _);
            }
        }

        /// <summary>
        /// Adds a new file entry or updates an existing entry with the specified content and priority, and optionally
        /// marks the entry as dirty for later processing.
        /// </summary>
        /// <remarks>If an entry for the specified path already exists, its content and priority are
        /// updated. Marking an entry as dirty may trigger additional processing, such as flushing changes to persistent
        /// storage.</remarks>
        /// <param name="path">The path of the file entry to add or update. Cannot be null or empty.</param>
        /// <param name="content">The byte array containing the content to associate with the file entry.
        /// Cannot be null.</param>
        /// <param name="dirty">A value indicating whether the entry should be marked as dirty for later
        /// processing. If <see langword="true"/>, the entry is flagged for flushing.</param>
        /// <param name="priority">The priority level to assign to the file entry. Determines the order in which
        /// entries are processed.</param>
        internal void SetEntry(string path, byte[] content, bool dirty, FilePriority priority)
        {
            var entry = _entries.AddOrUpdate(path,
                p => new FileEntry(p, content, priority),
                (_, existing) =>
                {
                    existing.Update(content, priority);
                    return existing;
                });

            if (entry.SizeBytes != content.Length)
            {
                Log.Warn($"Size mismatch for {path}: {entry.SizeBytes} vs {content.Length}");
            }
                
            if (dirty) 
            { 
                FlushQueue.MarkDirty(path, priority); 
            }
        } 
    }
    #endregion

    #region FlushQueue Component
    /// <summary>
    /// Provides static methods for tracking and flushing dirty file entries from in-memory caches to persistent storage
    /// based on priority and accumulated data size.
    /// </summary>
    /// <remarks>The FlushQueue component manages separate queues for different file priorities and monitors
    /// the total size of dirty (modified but not yet flushed) files. When the queued data exceeds a defined threshold,
    /// or during explicit flush operations, it writes all pending changes to disk. This class is intended for internal
    /// use and is not thread-safe for direct manipulation of its internal state outside its provided methods.</remarks>
    private static class FlushQueue
    {
        private static readonly ConcurrentDictionary<string, byte> High = new();
        private static readonly ConcurrentDictionary<string, byte> Low = new();
        private static readonly ConcurrentDictionary<string, byte> Game = new();

        private static long _queuedBytesHigh;
        private static long _queuedBytesLow;
        private static long _queuedBytesGame;

        /// <summary>
        /// Marks the specified file path as dirty, indicating that it requires processing at the given priority level.
        /// </summary>
        /// <param name="path">The file system path to be marked as dirty. Cannot be null or empty.</param>
        /// <param name="priority">The priority level to assign to the dirty file path. Determines
        /// the processing order.</param>
        internal static void MarkDirty(string path, FilePriority priority)
        {
            var queue = GetQueue(priority);

            if (!queue.TryAdd(path, 0))
                return;

            if (GetCache(priority).TryGetValue(path, out FileEntry? entry) && entry != null)
            {
                ref var queuedBytes = ref GetQueuedBytesRef(priority);
                Interlocked.Add(ref queuedBytes, entry.SizeBytes);
            }
        }

        /// <summary>
        /// Determines whether the specified path has unsaved changes in any tracked data set.
        /// </summary>
        /// <param name="path">The path to check for unsaved changes. Cannot be null.</param>
        /// <returns>true if the specified path is marked as dirty in any data set; otherwise, false.</returns>
        internal static bool IsDirty(string path) =>
            (High.ContainsKey(path) || Low.ContainsKey(path) || Game.ContainsKey(path));

        /// <summary>
        /// Gets the list of dirty file paths for a specific priority
        /// </summary>
        internal static IEnumerable<string> GetDirtyPaths(FilePriority priority)
        {
            return GetQueue(priority).Keys;
        }

        /// <summary>
        /// Retrieves the queue associated with the specified file priority.
        /// </summary>
        /// <param name="priority">The priority level for which to retrieve the corresponding queue.
        /// Must be a defined value of <see cref="FilePriority"/>.</param>
        /// <returns>A <see cref="ConcurrentDictionary{TKey, TValue}"/> representing the queue
        /// for the specified priority.</returns>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="priority"/> is
        /// not a recognized value of <see cref="FilePriority"/>.</exception>
        private static ConcurrentDictionary<string, byte> GetQueue(FilePriority priority)
        {
            return priority switch
            {
                FilePriority.SystemFast => High,
                FilePriority.SystemSlow => Low,
                FilePriority.Game => Game,
                _ => throw new NotImplementedException($"Unknown priority: {priority}"),
            };
        }

        /// <summary>
        /// Retrieves the cache instance associated with the specified file priority.
        /// </summary>
        /// <param name="priority">The file priority for which to acquire the corresponding cache.
        /// Must be a defined value of <see cref="FilePriority"/>.</param>
        /// <returns>The <see cref="Cache"/> instance that matches the specified file priority.</returns>
        /// <exception cref="NullReferenceException">Thrown if the cache instance for the specified
        /// priority has not been initialized.</exception>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="priority"/> is not a
        /// recognized value of <see cref="FilePriority"/>.</exception>
        private static Cache GetCache(FilePriority priority)
        {
            return priority switch
            {
                FilePriority.SystemFast => _cacheFast
                    ?? throw new NullReferenceException("_cacheFast is null"),
                FilePriority.SystemSlow => _cacheSlow
                    ?? throw new NullReferenceException("_cacheSlow is null"),
                FilePriority.Game => _cacheGame
                    ?? throw new NullReferenceException("_cacheGame is null"),
                _ => throw new NotImplementedException($"Unknown priority: {priority}"),
            };
        }

        /// <summary>
        /// Calculates the total number of bytes for all cached file entries currently marked as dirty.
        /// </summary>
        /// <remarks>This method aggregates the sizes of file entries found in multiple internal cache
        /// dictionaries. The result reflects the current state of the cache and may change as entries are added or
        /// removed.</remarks>
        /// <returns>The total size, in bytes, of all dirty cached file entries. Returns 0 if
        /// no entries are dirty.</returns>
        internal static long CalculateDirtyBytes()
        {
            if (_cacheFast is null || _cacheSlow is null || _cacheGame is null)
            {
                Log.Error("Cache not initialized in CalculateDirtyBytes.");
                return 0;
            }

            return SumSizes(High.Keys, _cacheFast)
                + SumSizes(Low.Keys, _cacheSlow)
                + SumSizes(Game.Keys, _cacheGame);
        }

        /// <summary>
        /// Calculates the total size, in bytes, of all cache entries corresponding to the specified keys.
        /// </summary>
        /// <param name="keys">A collection of keys identifying the cache entries whose sizes will be summed.
        /// Only keys present in the cache are considered.</param>
        /// <param name="cache">The cache instance containing the entries to be evaluated.</param>
        /// <returns>The sum of the sizes, in bytes, of all cache entries found for the specified keys.
        /// Returns 0 if none of the keys are present in the cache.</returns>
        private static long SumSizes(IEnumerable<string> keys, Cache cache)
        {
            long sum = 0;

            foreach (string key in keys)
            {
                if (cache.TryGetValue(key, out FileEntry? entry) && entry is not null)
                    sum += entry.SizeBytes;
            }

            return sum;
        }

        /// <summary>
        /// Checks the state of a specific priority queue and flushes it if necessary.
        /// </summary>
        /// <remarks>This method flushes only the specified priority queue if its threshold is exceeded.
        /// Each priority maintains its own flush cadence.</remarks>
        /// <param name="priority">The priority queue to check and potentially flush.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        internal static async Task CheckFlush(FilePriority priority)
        {
            var queue = GetQueue(priority);

            if (queue.IsEmpty)
                return;

            if (
                Interlocked.Read(ref GetQueuedBytesRef(priority))
                >= Constellations.FlushthresholdBytes
            )
            {
                await FlushQueueInternal(queue, GetCache(priority));
                Interlocked.Exchange(ref GetQueuedBytesRef(priority), 0);
            }
        }

        /// <summary>
        /// Returns a reference to the queued bytes counter associated with the specified file priority.
        /// </summary>
        /// <remarks>This method enables direct modification of the queued bytes counter for the specified
        /// priority. Use with caution, as changes to the referenced value affect the global state.</remarks>
        /// <param name="priority">The file priority for which to retrieve the queued bytes reference.
        /// Must be one of the defined values in <see cref="FilePriority"/>.</param>
        /// <returns>A reference to the <see langword="long"/> value representing the number of queued bytes for the given
        /// priority.</returns>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="priority"/> is not a recognized <see cref="FilePriority"/> value.</exception>
        private static ref long GetQueuedBytesRef(FilePriority priority)
        {
            if (priority == FilePriority.SystemFast)
                return ref _queuedBytesHigh;
            if (priority == FilePriority.SystemSlow)
                return ref _queuedBytesLow;
            if (priority == FilePriority.Game)
                return ref _queuedBytesGame;

            throw new NotImplementedException($"Unknown priority: {priority}");
        }

        /// <summary>
        /// Asynchronously flushes all pending items from the internal high, game, and low-priority queues.
        /// </summary>
        /// <remarks>This method resets the internal queued byte count after flushing all queues. It is
        /// intended for internal use and should not be called directly from external code.</remarks>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        internal static async Task FlushAll()
        {
            if (_cacheFast == null || _cacheSlow == null || _cacheGame == null)
            {
                Log.Error("Caches not initialized in FlushAll!");
                return;
            }

            await FlushQueueInternal(High, _cacheFast);
            await FlushQueueInternal(Low, _cacheSlow);
            await FlushQueueInternal(Game, _cacheGame);

            Interlocked.Exchange(ref _queuedBytesHigh, 0);
            Interlocked.Exchange(ref _queuedBytesLow, 0);
            Interlocked.Exchange(ref _queuedBytesGame, 0);
        }

        /// <summary>
        /// Asynchronously flushes all pending file entries from the specified queue to disk using the provided cache.
        /// </summary>
        /// <remarks>Files are written to disk only if their content is available and non-empty. If a
        /// file's version changes during the write operation, it remains in the queue for a future flush. Any errors
        /// encountered during writing are logged, and affected files remain queued for retry.</remarks>
        /// <param name="queue">A thread-safe dictionary containing file paths queued for flushing. Each key represents a file path to be
        /// written.</param>
        /// <param name="cache">The cache containing file entries to be flushed. Used to retrieve the content and version information for
        /// each file.</param>
        /// <returns>A task that represents the asynchronous flush operation. The task completes when all eligible files have
        /// been processed.</returns>
        private static async Task FlushQueueInternal(
            ConcurrentDictionary<string, byte> queue,
            Cache cache
        )
        {
            if (queue.IsEmpty)
                return;

            // Snapshot keys for safe enumeration
            string[] keys = [.. queue.Keys];

            foreach (string path in keys)
            {
                if (!cache.TryGetValue(path, out FileEntry? entry) || entry == null)
                    continue;

                var currentVersion = entry.Version;
                var data = entry.Content;

                if (data == null || data.Length == 0)
                    continue;

                try
                {
                    await File.WriteAllBytesAsync(path, data);

                    if (entry.Version == currentVersion)
                    {
                        queue.TryRemove(path, out _);
                    }
                    // If a version has changed, it means it was updated again while we were writing
                    // We'll pick it up on the next tick.
                }
                catch (Exception ex)
                {
                    Log.Error($"Flush failed for {path}: {ex.Message}", ex);
                    // File remains in the queue for retry on the next flush
                }
            }
        }
    }
    #endregion
}

/*
*------------------------------------------------------------
* (FileDaemon.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/
