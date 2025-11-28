/*
 * (FileEntryTests.cs)
 *------------------------------------------------------------
 * Created - 11/26/2025 3:03:46 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using FluffyByte.MUD.Driver.Core.Types.Daemons.FileManager;

namespace FluffyByte.MUD.Test;

public class FileEntryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithPathOnly_CreatesEntryWithDefaults()
    {
        var entry = new FileEntry("/game/test.txt");

        Assert.Equal("/game/test.txt", entry.Path);
        Assert.Null(entry.Content);
        Assert.Equal(FilePriority.Game, entry.Priority);
        Assert.Equal(0, entry.Version);
        Assert.Equal(0, entry.SizeBytes);
    }

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        byte[] content = [0x48, 0x65, 0x6C, 0x6C, 0x6F]; // "Hello"

        var entry = new FileEntry("/game/test.txt", content);

        Assert.Equal(content, entry.Content);
        Assert.Equal(5, entry.SizeBytes);
    }

    [Fact]
    public void Constructor_WithPriority_SetsPriority()
    {
        var entry = new FileEntry("/system/config.txt", null, FilePriority.SystemFast);

        Assert.Equal(FilePriority.SystemFast, entry.Priority);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        byte[] content = [0x01, 0x02, 0x03];

        var entry = new FileEntry("/user/data.bin", content);

        Assert.Equal("/user/data.bin", entry.Path);
        Assert.Equal(content, entry.Content);
        Assert.Equal(FilePriority.Game, entry.Priority);
        Assert.Equal(3, entry.SizeBytes);
    }

    [Fact]
    public void Constructor_SetsLastAccessToUtcNow()
    {
        var before = DateTime.UtcNow;

        var entry = new FileEntry("/game/test.txt");

        var after = DateTime.UtcNow;

        Assert.InRange(entry.LastAccess, before, after);
    }

    [Fact]
    public void Constructor_WithNullContent_SizeBytesIsZero()
    {
        var entry = new FileEntry("/game/test.txt");

        Assert.Equal(0, entry.SizeBytes);
    }

    [Fact]
    public void Constructor_WithEmptyContent_SizeBytesIsZero()
    {
        var entry = new FileEntry("/game/test.txt", []);

        Assert.Equal(0, entry.SizeBytes);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithNewContent_ReplacesContent()
    {
        byte[] originalContent = [0x01, 0x02];
        byte[] newContent = [0x03, 0x04, 0x05];
        var entry = new FileEntry("/game/test.txt", originalContent);

        entry.Update(newContent);

        Assert.Equal(newContent, entry.Content);
        Assert.Equal(3, entry.SizeBytes);
    }

    [Fact]
    public void Update_IncrementsVersion()
    {
        var entry = new FileEntry("/game/test.txt");
        Assert.Equal(0, entry.Version);

        entry.Update([0x01]);
        Assert.Equal(1, entry.Version);

        entry.Update([0x02]);
        Assert.Equal(2, entry.Version);

        entry.Update([0x03]);
        Assert.Equal(3, entry.Version);
    }

    [Fact]
    public void Update_WithPriority_UpdatesPriority()
    {
        var entry = new FileEntry("/game/test.txt");

        entry.Update([0x01], FilePriority.SystemSlow);

        Assert.Equal(FilePriority.SystemSlow, entry.Priority);
    }

    [Fact]
    public void Update_WithoutPriority_KeepsPriority()
    {
        var entry = new FileEntry("/game/test.txt");

        entry.Update([0x01]);

        Assert.Equal(FilePriority.Game, entry.Priority);
    }

    [Fact]
    public void Update_WithNullPriority_KeepsPriority()
    {
        var entry = new FileEntry("/game/test.txt", null, FilePriority.SystemFast);

        entry.Update([0x01]);

        Assert.Equal(FilePriority.SystemFast, entry.Priority);
    }

    [Fact]
    public void Update_UpdatesLastAccess()
    {
        var entry = new FileEntry("/game/test.txt");
        var originalAccess = entry.LastAccess;

        Thread.Sleep(15); // Ensure time difference

        entry.Update([0x01]);

        Assert.True(entry.LastAccess > originalAccess);
    }

    [Fact]
    public void Update_LastAccessIsUtcNow()
    {
        var entry = new FileEntry("/game/test.txt");

        var before = DateTime.UtcNow;
        entry.Update([0x01]);
        var after = DateTime.UtcNow;

        Assert.InRange(entry.LastAccess, before, after);
    }

    #endregion

    #region Touch Tests

    [Fact]
    public void Touch_UpdatesLastAccess()
    {
        var entry = new FileEntry("/game/test.txt");
        var originalAccess = entry.LastAccess;

        Thread.Sleep(15); // Ensure time difference

        entry.Touch();

        Assert.True(entry.LastAccess > originalAccess);
    }

    [Fact]
    public void Touch_LastAccessIsUtcNow()
    {
        var entry = new FileEntry("/game/test.txt");

        var before = DateTime.UtcNow;
        entry.Touch();
        var after = DateTime.UtcNow;

        Assert.InRange(entry.LastAccess, before, after);
    }

    [Fact]
    public void Touch_DoesNotAffectVersion()
    {
        var entry = new FileEntry("/game/test.txt");
        var originalVersion = entry.Version;

        entry.Touch();

        Assert.Equal(originalVersion, entry.Version);
    }

    [Fact]
    public void Touch_DoesNotAffectContent()
    {
        byte[] content = [0x01, 0x02, 0x03];
        var entry = new FileEntry("/game/test.txt", content);

        entry.Touch();

        Assert.Equal(content, entry.Content);
    }

    [Fact]
    public void Touch_DoesNotAffectPriority()
    {
        var entry = new FileEntry("/game/test.txt", null, FilePriority.SystemSlow);

        entry.Touch();

        Assert.Equal(FilePriority.SystemSlow, entry.Priority);
    }

    #endregion

    #region SizeBytes Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1024)]
    [InlineData(65536)]
    public void SizeBytes_ReturnsContentLength(int size)
    {
        byte[] content = new byte[size];
        var entry = new FileEntry("/game/test.txt", content);

        Assert.Equal(size, entry.SizeBytes);
    }

    [Fact]
    public void SizeBytes_UpdatesAfterContentChange()
    {
        var entry = new FileEntry("/game/test.txt", new byte[10]);
        Assert.Equal(10, entry.SizeBytes);

        entry.Update(new byte[50]);
        Assert.Equal(50, entry.SizeBytes);

        entry.Update(new byte[5]);
        Assert.Equal(5, entry.SizeBytes);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Update_ConcurrentUpdates_VersionIsConsistent()
    {
        var entry = new FileEntry("/game/test.txt");
        const int updateCount = 1000;
        const int threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < updateCount; i++)
                {
                    entry.Update([(byte)(i % 256)]);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(updateCount * threadCount, entry.Version);
    }

    [Fact]
    public async Task Update_ConcurrentUpdates_ContentIsValid()
    {
        var entry = new FileEntry("/game/test.txt");
        const int iterations = 100;
        const int threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(threadId => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    byte[] content = [(byte)threadId, (byte)i];
                    entry.Update(content);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Content should be a valid 2-byte array from one of the threads
        Assert.NotNull(entry.Content);
        Assert.Equal(2, entry.Content.Length);
    }

    [Fact]
    public async Task Update_ConcurrentWithPriorityChanges_PriorityIsValid()
    {
        var entry = new FileEntry("/game/test.txt");
        var priorities = Enum.GetValues<FilePriority>();
        const int iterations = 100;

        var tasks = priorities.Select(priority => Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                entry.Update([0x01], priority);
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Priority should be one of the valid values
        Assert.Contains(entry.Priority, priorities);
    }

    [Fact]
    public async Task Touch_ConcurrentCalls_NoExceptions()
    {
        var entry = new FileEntry("/game/test.txt");
        const int iterations = 1000;
        const int threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    entry.Touch();
                }
            }))
            .ToArray();

        // Should complete without throwing
        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentUpdateAndTouch_NoExceptions()
    {
        var entry = new FileEntry("/game/test.txt");
        const int iterations = 500;

        var updateTask = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                entry.Update([(byte)i]);
            }
        });

        var touchTask = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                entry.Touch();
            }
        });

        await Task.WhenAll(updateTask, touchTask);

        Assert.Equal(iterations, entry.Version);
    }

    #endregion

    #region Path Tests

    [Theory]
    [InlineData("/game/test.txt")]
    [InlineData("/system/config/settings.json")]
    [InlineData("relative/path/file.bin")]
    [InlineData("C:\\Windows\\Style\\Path.exe")]
    [InlineData("/deeply/nested/directory/structure/file.txt")]
    public void Path_PreservesOriginalValue(string path)
    {
        var entry = new FileEntry(path);

        Assert.Equal(path, entry.Path);
    }

    [Fact]
    public void Path_IsImmutable()
    {
        var entry = new FileEntry("/original/path.txt");

        // Path has no setter, so this verifies immutability by design
        Assert.Equal("/original/path.txt", entry.Path);

        entry.Update([0x01]);
        Assert.Equal("/original/path.txt", entry.Path);

        entry.Touch();
        Assert.Equal("/original/path.txt", entry.Path);
    }

    #endregion

    #region Priority Tests

    [Theory]
    [InlineData(FilePriority.SystemSlow)]
    [InlineData(FilePriority.Game)]
    [InlineData(FilePriority.SystemFast)]
    public void Priority_AcceptsAllEnumValues(FilePriority priority)
    {
        var entry = new FileEntry("/test.txt", null, priority);

        Assert.Equal(priority, entry.Priority);
    }

    [Theory]
    [InlineData(FilePriority.SystemFast)]
    [InlineData(FilePriority.Game)]
    [InlineData(FilePriority.SystemSlow)]
    public void Update_CanChangePriorityToAnyValue(FilePriority newPriority)
    {
        var entry = new FileEntry("/test.txt");

        entry.Update([0x01], newPriority);

        Assert.Equal(newPriority, entry.Priority);
    }

    #endregion

    #region Version Tests

    [Fact]
    public void Version_StartsAtZero()
    {
        var entry = new FileEntry("/test.txt");

        Assert.Equal(0, entry.Version);
    }

    [Fact]
    public void Version_OnlyIncrementsOnUpdate()
    {
        var entry = new FileEntry("/test.txt");

        entry.Touch();
        Assert.Equal(0, entry.Version);

        entry.Touch();
        Assert.Equal(0, entry.Version);

        entry.Update([0x01]);
        Assert.Equal(1, entry.Version);
    }

    [Fact]
    public void Version_IncrementsMonotonically()
    {
        var entry = new FileEntry("/test.txt");

        for (int i = 1; i <= 100; i++)
        {
            entry.Update([(byte)i]);
            Assert.Equal(i, entry.Version);
        }
    }

    #endregion

    #region Content Edge Cases

    [Fact]
    public void Content_CanStoreLargeContent()
    {
        byte[] largeContent = new byte[1024 * 1024]; // 1 MB
        new Random(42).NextBytes(largeContent);

        var entry = new FileEntry("/large/file.bin", largeContent);

        Assert.Equal(largeContent, entry.Content);
        Assert.Equal(1024 * 1024, entry.SizeBytes);
    }

    [Fact]
    public void Content_CanUpdateWithLargerContent()
    {
        var entry = new FileEntry("/test.txt", new byte[10]);

        entry.Update(new byte[1000]);

        Assert.Equal(1000, entry.SizeBytes);
    }

    [Fact]
    public void Content_CanUpdateWithSmallerContent()
    {
        var entry = new FileEntry("/test.txt", new byte[1000]);

        entry.Update(new byte[10]);

        Assert.Equal(10, entry.SizeBytes);
    }

    [Fact]
    public void Content_CanUpdateWithEmptyContent()
    {
        var entry = new FileEntry("/test.txt", new byte[100]);

        entry.Update([]);

        Assert.Empty(entry.Content!);
        Assert.Equal(0, entry.SizeBytes);
    }

    [Fact]
    public void Content_PreservesBinaryData()
    {
        byte[] binaryData = [0x00, 0xFF, 0x7F, 0x80, 0x01, 0xFE];
        var entry = new FileEntry("/test.bin", binaryData);

        Assert.Equal(binaryData, entry.Content);
    }

    [Fact]
    public void Content_CanRepresentTextAsBytes()
    {
        string text = "Hello, World!";
        byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        var entry = new FileEntry("/test.txt", textBytes);

        string decoded = System.Text.Encoding.UTF8.GetString(entry.Content!);
        Assert.Equal(text, decoded);
    }

    #endregion

    #region LastAccess Behavior

    [Fact]
    public void LastAccess_IsUtcKind()
    {
        var entry = new FileEntry("/test.txt");

        Assert.Equal(DateTimeKind.Utc, entry.LastAccess.Kind);
    }

    [Fact]
    public void LastAccess_IsUtcAfterTouch()
    {
        var entry = new FileEntry("/test.txt");

        entry.Touch();

        Assert.Equal(DateTimeKind.Utc, entry.LastAccess.Kind);
    }

    [Fact]
    public void LastAccess_IsUtcAfterUpdate()
    {
        var entry = new FileEntry("/test.txt");

        entry.Update([0x01]);

        Assert.Equal(DateTimeKind.Utc, entry.LastAccess.Kind);
    }

    [Fact]
    public void LastAccess_ReadingContentDoesNotUpdateAccess()
    {
        var entry = new FileEntry("/test.txt", [0x01, 0x02, 0x03]);
        var originalAccess = entry.LastAccess;

        Thread.Sleep(15);

        _ = entry.Content;
        _ = entry.SizeBytes;

        Assert.Equal(originalAccess, entry.LastAccess);
    }

    #endregion
}

/*
*------------------------------------------------------------
* (FileEntryTests.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/