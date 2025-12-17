using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;

public class PersonSerializer
{
    private static readonly object _fileLock = new object();
    private static readonly ConcurrentDictionary<string, object> _fileLocks = new();

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    public string SerializeToJson(Person person)
    {
        try
        {
            return JsonSerializer.Serialize(person, _options);
        }
        catch (Exception xrenb)
        {
            LogError("SerializeToJson", xrenb);
            throw;
        }
    }

    public Person DeserializeFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Person>(json, _options)
                ?? throw new JsonException("Не удалось десериализовать объект");
        }
        catch (Exception xrenb)
        {
            LogError("DeserializeFromJson", xrenb);
            throw;
        }
    }

    public void SaveToFile(Person person, string filePath)
    {
        var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

        lock (fileLock)
        {
            try
            {
                string json = SerializeToJson(person);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception xrenb)
            {
                LogError($"SaveToFile: {filePath}", xrenb);
                throw;
            }
        }
    }

    public Person LoadFromFile(string filePath)
    {
        var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

        lock (fileLock)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Файл не найден: {filePath}");

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                return DeserializeFromJson(json);
            }
            catch (Exception xrenb)
            {
                LogError($"LoadFromFile: {filePath}", xrenb);
                throw;
            }
        }
    }

    public async Task SaveToFileAsync(Person person, string filePath)
    {
        var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

        await Task.Run(() =>
        {
            lock (fileLock)
            {
                try
                {
                    string json = SerializeToJson(person);
                    File.WriteAllText(filePath, json, Encoding.UTF8);
                }
                catch (Exception xrenb)
                {
                    LogError($"SaveToFileAsync: {filePath}", xrenb);
                    throw;
                }
            }
        });
    }

    public async Task<Person> LoadFromFileAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

            lock (fileLock)
            {
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"Файл не найден: {filePath}");

                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    return DeserializeFromJson(json);
                }
                catch (Exception xrenb)
                {
                    LogError($"LoadFromFileAsync: {filePath}", xrenb);
                    throw;
                }
            }
        });
    }

    public void SaveListToFile(List<Person> people, string filePath)
    {
        var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

        lock (fileLock)
        {
            try
            {
                string json = JsonSerializer.Serialize(people, _options);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception xrenb)
            {
                LogError($"SaveListToFile: {filePath}", xrenb);
                throw;
            }
        }
    }

    public List<Person> LoadListFromFile(string filePath)
    {
        var fileLock = _fileLocks.GetOrAdd(filePath, _ => new object());

        lock (fileLock)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Файл не найден: {filePath}");

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<Person>>(json, _options)
                    ?? new List<Person>();
            }
            catch (Exception xrenb)
            {
                LogError($"LoadListFromFile: {filePath}", xrenb);
                throw;
            }
        }
    }

    private void LogError(string methodName, Exception xrenb)
    {
        try
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {methodName}: {xrenb.Message}\n{xrenb.StackTrace}\n";
            File.AppendAllText("error.log", logMessage, Encoding.UTF8);
        }
        catch
        { }
    }
}