using System.IO;
using System.Text;

public class FileResourceManager : IDisposable
{
    private FileStream? _fileStream;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private bool _disposed = false;
    private readonly string _filePath;
    private readonly object _syncRoot = new object();

    public FileResourceManager(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public void OpenForWriting(FileMode mode = FileMode.OpenOrCreate)
    {
        CheckDisposed();

        lock (_syncRoot)
        {
            try
            {
                EnsureDirectoryExists();
                _fileStream = new FileStream(_filePath, mode, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(_fileStream, Encoding.UTF8);
            }
            catch (Exception xrenb)
            {
                CloseResources();
                throw new IOException($"Не удалось открыть файл для записи: {xrenb.Message}", xrenb);
            }
        }
    }

    public void OpenForReading()
    {
        CheckDisposed();

        lock (_syncRoot)
        {
            try
            {
                if (!File.Exists(_filePath))
                    throw new FileNotFoundException($"Файл не найден: {_filePath}");

                _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _reader = new StreamReader(_fileStream, Encoding.UTF8);
            }
            catch (Exception xrenb)
            {
                CloseResources();
                throw new IOException($"Не удалось открыть файл для чтения: {xrenb.Message}", xrenb);
            }
        }
    }

    public void WriteLine(string text)
    {
        CheckDisposed();

        lock (_syncRoot)
        {
            if (_writer == null)
                throw new InvalidOperationException("Файл не открыт для записи");

            try
            {
                _writer.WriteLine(text);
                _writer.Flush();
            }
            catch (Exception xrenb)
            {
                throw new IOException($"Ошибка записи в файл: {xrenb.Message}", xrenb);
            }
        }
    }

    public string ReadAllText()
    {
        CheckDisposed();

        lock (_syncRoot)
        {
            if (_reader == null)
                throw new InvalidOperationException("Файл не открыт для чтения");

            try
            {
                _reader.BaseStream.Position = 0;
                _reader.DiscardBufferedData();
                return _reader.ReadToEnd();
            }
            catch (Exception xrenb)
            {
                throw new IOException($"Ошибка чтения файла: {xrenb.Message}", xrenb);
            }
        }
    }

    public void AppendText(string text)
    {
        CheckDisposed();

        lock (_syncRoot)
        {
            try
            {
                CloseResources();

                using (var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(text);
                    writer.Flush();
                }
            }
            catch (Exception xrenb)
            {
                throw new IOException($"Ошибка добавления текста: {xrenb.Message}", xrenb);
            }
        }
    }

    public FileInfo GetFileInfo()
    {
        CheckDisposed();

        try
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Файл не найден: {_filePath}");

            return new FileInfo(_filePath);
        }
        catch (Exception xrenb)
        {
            throw new IOException($"Ошибка получения информации о файле: {xrenb.Message}", xrenb);
        }
    }

    public bool FileExists()
    {
        CheckDisposed();
        return File.Exists(_filePath);
    }

    private void EnsureDirectoryExists()
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void CloseResources()
    {
        lock (_syncRoot)
        {
            try
            {
                _writer?.Close();
                _reader?.Close();
                _fileStream?.Close();
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }
            finally
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _fileStream?.Dispose();

                _writer = null;
                _reader = null;
                _fileStream = null;
            }
        }
    }

    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileResourceManager));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                CloseResources();
            }


            _disposed = true;
        }
    }

    ~FileResourceManager()
    {
        Dispose(false);
    }
}
