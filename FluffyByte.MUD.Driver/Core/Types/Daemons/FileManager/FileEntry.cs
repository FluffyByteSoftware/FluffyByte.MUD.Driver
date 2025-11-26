/*
 * (FileEntry.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 5:31:08 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

namespace FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

public sealed class FileEntry(string path, byte[] content)
{
    private readonly Lock _lock = new();

    public string Path { get; } = path;
    public byte[] Content { get; private set; } = content;

    public int SizeBytes => Content.Length;

    public void Update(byte[] newContent)
    {
        lock (_lock)
        {
            Content = newContent;
        }
    }
}

/*
*------------------------------------------------------------
* (FileEntry.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/