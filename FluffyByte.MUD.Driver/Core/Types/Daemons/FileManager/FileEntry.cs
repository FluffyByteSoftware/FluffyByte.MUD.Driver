/*
 * (FileEntry.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 5:31:08 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */
namespace FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

/// <summary>Represents a file entry containing path information, content, versioning, and metadata related to the
/// file.</summary>
/// <remarks>This class is immutable with regard to its path property and leverages locking to ensure
/// thread-safe updates to its state. It supports operations such as updating file content and tracking access time.
/// </remarks>
public sealed class FileEntry(string path, byte[]? content = null)
{
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the file system path associated with this instance.
    /// </summary>
    public string Path { get; } = path;
    /// <summary>
    /// Gets the binary content associated with this instance.
    /// </summary>
    private byte[]? Content { get; set; } = content;

    /// <summary>
    /// Gets the current version number associated with the instance.
    /// </summary>
    private long Version { get; set; }
    
    /// <summary>Gets the size of the content, in bytes. </summary>
    public int SizeBytes => Content?.Length ?? 0;

    /// <summary>Updates the content of the file entry and increments its version number.</summary>
    /// <remarks>This method replaces the current content with the provided new content, increments the version
    /// to reflect the update, and sets the last access time to the current UTC time. Thread safety is ensured using
    /// a locking mechanism.</remarks>
    /// <param name="newContent">The new content to replace the existing content of the file entry.</param>
    public void Update(byte[] newContent)
    {
        lock (_lock)
        {
            Content = newContent;
            Version++;
        }
    }
}
/*
*------------------------------------------------------------
* (FileEntry.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/