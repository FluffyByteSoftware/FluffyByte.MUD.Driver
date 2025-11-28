/*
 * (FileEntry.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 5:31:08 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
namespace FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

/// <summary>
/// Represents a file entry with associated path, binary content, priority, and access metadata.
/// </summary>
/// <remarks>Use this class to encapsulate file data and metadata for scenarios such as virtual file systems,
/// caching, or prioritized file management. The class is thread-safe for content and priority updates. Access time is
/// tracked in UTC and updated on modification or when explicitly touched.</remarks>
/// <param name="path">The file system path to associate with this entry. Cannot be null or empty.</param>
/// <param name="content">The binary content of the file. If null, the entry is created without content.</param>
/// <param name="priority">The priority level to assign to the file. Defaults to FilePriority.Game if not specified.</param>
public sealed class FileEntry(string path, 
    byte[]? content = null, 
    FilePriority priority = FilePriority.Game)
{
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the file system path associated with this instance.
    /// </summary>
    public string Path { get; } = path;
    /// <summary>
    /// Gets the binary content associated with this instance.
    /// </summary>
    public byte[]? Content { get; private set; } = content;

    /// <summary>
    /// Gets the current version number associated with the instance.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Gets the priority level assigned to the file.
    /// </summary>
    public FilePriority Priority { get; private set; } = priority;
    /// <summary>
    /// Gets the date and time, in Coordinated Universal Time (UTC), when the item was last accessed.
    /// </summary>
    public DateTime LastAccess { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the size of the content, in bytes.
    /// </summary>
    public int SizeBytes => Content?.Length ?? 0;

    /// <summary>
    /// Updates the file's content and optionally sets its priority.
    /// </summary>
    /// <remarks>Updates the file's last access time to the current UTC time. This method is
    /// thread-safe.</remarks>
    /// <param name="newContent">The new content to assign to the file. Cannot be null.</param>
    /// <param name="priority">The priority to assign to the file. If null, the priority remains unchanged.</param>
    public void Update(byte[] newContent, FilePriority? priority = null)
    {
        lock (_lock)
        {
            Content = newContent;

            Version++;

            if (priority.HasValue)
                Priority = priority.Value;
        }

        LastAccess = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the last access time to the current UTC time.
    /// </summary>
    /// <remarks>Call this method to record that the object was accessed. This is typically used to track
    /// usage or implement expiration policies based on access time.</remarks>
    public void Touch()
    {
        LastAccess = DateTime.UtcNow;
    }
}

/*
*------------------------------------------------------------
* (FileEntry.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/