/*
 * (FileEntry.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 5:31:08 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

namespace FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

public sealed class FileEntry(string path, 
    byte[]? content = null, 
    FilePriority priority = FilePriority.Game)
{
    private readonly Lock _lock = new();

    public string Path { get; } = path;
    public byte[]? Content { get; private set; } = content;

    public FilePriority Priority { get; private set; } = priority;

    public int SizeBytes => Content?.Length ?? 0;

    public void Update(byte[] newContent, FilePriority? priority = null)
    {
        lock (_lock)
        {
            Content = newContent;

            if (priority.HasValue)
                Priority = priority.Value;
        }
    }
}

/*
*------------------------------------------------------------
* (FileEntry.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/