using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class FileResourceManagerTests
{
    [Fact]
    public void OpenForWriting_И_ReadAllText_ЧитаютТоЧтоЗаписали()
    {
        string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");

        try
        {
            using (var writerManager = new FileResourceManager(path))
            {
                writerManager.OpenForWriting();
                writerManager.WriteLine("Первая строка");
                writerManager.WriteLine("Вторая строка");
            }

            using (var readerManager = new FileResourceManager(path))
            {
                readerManager.OpenForReading();
                string content = readerManager.ReadAllText();

                string expected = $"Первая строка{Environment.NewLine}Вторая строка{Environment.NewLine}";
                Assert.Equal(expected, content);
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void AppendText_ДобавляетТекстВКонецФайла()
    {
        string path = Path.Combine(Path.GetTempPath(), $"append_{Guid.NewGuid()}.txt");

        try
        {
            using (var manager = new FileResourceManager(path))
            {
                manager.OpenForWriting();
                manager.WriteLine("Базовая строка");
            }

            using (var manager = new FileResourceManager(path))
            {
                manager.AppendText("Добавка");
            }

            using (var manager = new FileResourceManager(path))
            {
                manager.OpenForReading();
                string content = manager.ReadAllText();

                string expected = $"Базовая строка{Environment.NewLine}Добавка";
                Assert.Equal(expected, content);
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void OpenForWriting_СоздаетКаталогЕслиЕгоНет()
    {
        string dir  = Path.Combine(Path.GetTempPath(), $"dir_{Guid.NewGuid()}");
        string path = Path.Combine(dir, "file.txt");

        try
        {
            using (var manager = new FileResourceManager(path))
            {
                manager.OpenForWriting();
                manager.WriteLine("test");
            }

            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FileExists_ВозвращаетКорректноеЗначение()
    {
        string path = Path.Combine(Path.GetTempPath(), $"exists_{Guid.NewGuid()}.txt");

        using (var manager = new FileResourceManager(path))
        {
            Assert.False(manager.FileExists());

            manager.OpenForWriting();
            manager.WriteLine("data");
        }

        using (var manager = new FileResourceManager(path))
        {
            Assert.True(manager.FileExists());
        }

        if (File.Exists(path))
            File.Delete(path);
    }

    [Fact]
    public void GetFileInfo_ДляСуществующегоФайла_ВозвращаетИнформацию()
    {
        string path = Path.Combine(Path.GetTempPath(), $"info_{Guid.NewGuid()}.txt");

        try
        {
            using (var manager = new FileResourceManager(path))
            {
                manager.OpenForWriting();
                manager.WriteLine("12345");
            }

            using (var manager = new FileResourceManager(path))
            {
                var info = manager.GetFileInfo();

                Assert.True(info.Exists);
                Assert.True(info.Length > 0);
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void GetFileInfo_ДляНесуществующегоФайла_БросаетIOExceptionСFileNotFoundВнутри()
    {
        string path = Path.Combine(Path.GetTempPath(), $"noinfo_{Guid.NewGuid()}.txt");

        using var manager = new FileResourceManager(path);

        var ex = Assert.Throws<IOException>(() => manager.GetFileInfo());
        Assert.IsType<FileNotFoundException>(ex.InnerException);
    }

    [Fact]
    public void OpenForReading_НесуществующийФайл_БросаетIOExceptionСFileNotFoundВнутри()
    {
        string path = Path.Combine(Path.GetTempPath(), $"noread_{Guid.NewGuid()}.txt");

        using var manager = new FileResourceManager(path);

        var ex = Assert.Throws<IOException>(() => manager.OpenForReading());
        Assert.IsType<FileNotFoundException>(ex.InnerException);
    }

    [Fact]
    public void WriteLine_БезOpenForWriting_БросаетInvalidOperationException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"nowrite_{Guid.NewGuid()}.txt");

        using var manager = new FileResourceManager(path);

        Assert.Throws<InvalidOperationException>(() => manager.WriteLine("text"));
    }

    [Fact]
    public void ReadAllText_БезOpenForReading_БросаетInvalidOperationException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"noread2_{Guid.NewGuid()}.txt");

        using var manager = new FileResourceManager(path);

        Assert.Throws<InvalidOperationException>(() => manager.ReadAllText());
    }

    [Fact]
    public void ПослеDispose_ЛюбойМетодБросаетObjectDisposedException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"disposed_{Guid.NewGuid()}.txt");

        var manager = new FileResourceManager(path);
        manager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => manager.OpenForWriting());
        Assert.Throws<ObjectDisposedException>(() => manager.OpenForReading());
        Assert.Throws<ObjectDisposedException>(() => manager.FileExists());
    }
}