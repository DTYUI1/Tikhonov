using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class PersonSerializerTests
{
    private readonly PersonSerializer _serializer = new();

    [Fact]
    public void SerializeToJson_НеСодержитПарольИСодержитПравильныеИменаСвойств()
    {
        var birthDate = new DateTime(1993, 10, 20);

        var person = new Person
        {
            FirstName  = "Мария",
            LastName   = "Петрова",
            Age        = 30,
            Password   = "secret123",
            Id         = "67890",
            BirthDate  = birthDate,
            Email      = "maria@example.com",
            PhoneNumber = "+7-912-345-67-89"
        };

        string json = _serializer.SerializeToJson(person);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Мария", root.GetProperty("FirstName").GetString());
        Assert.Equal("Петрова", root.GetProperty("LastName").GetString());
        Assert.Equal(30, root.GetProperty("Age").GetInt32());

        // Переименование свойств
        Assert.Equal("67890", root.GetProperty("personId").GetString());
        Assert.Equal("+7-912-345-67-89", root.GetProperty("phone").GetString());

        // Поле _birthDate должно сериализоваться благодаря [JsonInclude]
        Assert.True(root.TryGetProperty("_birthDate", out var birthDateProp));
        Assert.Equal(birthDate, birthDateProp.GetDateTime());

        // Пароля быть не должно
        Assert.False(root.TryGetProperty("Password", out _));
        Assert.DoesNotContain("secret123", json);
    }

    [Fact]
    public void DeserializeFromJson_КорректноВосстанавливаетОбъект()
    {
        const string json = """
        {
          "FirstName": "Мария",
          "LastName": "Петрова",
          "Age": 30,
          "personId": "67890",
          "_birthDate": "1993-10-20T00:00:00",
          "Email": "maria@example.com",
          "phone": "+7-912-345-67-89"
        }
        """;

        var person = _serializer.DeserializeFromJson(json);

        Assert.Equal("Мария", person.FirstName);
        Assert.Equal("Петрова", person.LastName);
        Assert.Equal(30, person.Age);
        Assert.Equal("67890", person.Id);
        Assert.Equal(new DateTime(1993, 10, 20), person.BirthDate);
        Assert.Equal("maria@example.com", person.Email);
        Assert.Equal("+7-912-345-67-89", person.PhoneNumber);
    }

    [Fact]
    public void SaveToFile_И_LoadFromFile_РаботаютКорректно()
    {
        var person = new Person
        {
            FirstName = "Мария",
            LastName  = "Петрова",
            Age       = 30,
            Id        = "67890",
            BirthDate = new DateTime(1993, 10, 20),
            Email     = "maria@example.com",
            PhoneNumber = "+7-912-345-67-89"
        };

        string path = Path.Combine(Path.GetTempPath(), $"person_{Guid.NewGuid()}.json");

        try
        {
            _serializer.SaveToFile(person, path);
            Assert.True(File.Exists(path));

            var loaded = _serializer.LoadFromFile(path);

            Assert.Equal(person.FirstName,  loaded.FirstName);
            Assert.Equal(person.LastName,   loaded.LastName);
            Assert.Equal(person.Age,        loaded.Age);
            Assert.Equal(person.Id,         loaded.Id);
            Assert.Equal(person.BirthDate,  loaded.BirthDate);
            Assert.Equal(person.Email,      loaded.Email);
            Assert.Equal(person.PhoneNumber,loaded.PhoneNumber);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task SaveToFileAsync_И_LoadFromFileAsync_РаботаютКорректно()
    {
        var person = new Person
        {
            FirstName = "Иван",
            LastName  = "Иванов",
            Age       = 25,
            Id        = "12345",
            BirthDate = new DateTime(1998, 5, 15),
            Email     = "ivan@example.com",
            PhoneNumber = "+7-999-123-45-67"
        };

        string path = Path.Combine(Path.GetTempPath(), $"person_async_{Guid.NewGuid()}.json");

        try
        {
            await _serializer.SaveToFileAsync(person, path);
            Assert.True(File.Exists(path));

            var loaded = await _serializer.LoadFromFileAsync(path);

            Assert.Equal(person.FullName,  loaded.FullName);
            Assert.Equal(person.Email,     loaded.Email);
            Assert.Equal(person.PhoneNumber, loaded.PhoneNumber);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LoadFromFile_ЕслиФайлаНет_БросаетFileNotFoundException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"no_such_{Guid.NewGuid()}.json");
        Assert.False(File.Exists(path));

        Assert.Throws<FileNotFoundException>(() => _serializer.LoadFromFile(path));
    }

    [Fact]
    public void SaveListToFile_И_LoadListFromFile_РаботаютКорректно()
    {
        var people = new List<Person>
        {
            new()
            {
                FirstName = "Мария",
                LastName  = "Петрова",
                Age       = 30,
                Email     = "maria@example.com"
            },
            new()
            {
                FirstName = "Алексей",
                LastName  = "Сидоров",
                Age       = 22,
                Email     = "alex@example.com"
            }
        };

        string path = Path.Combine(Path.GetTempPath(), $"people_{Guid.NewGuid()}.json");

        try
        {
            _serializer.SaveListToFile(people, path);

            var loaded = _serializer.LoadListFromFile(path);

            Assert.Equal(2, loaded.Count);
            Assert.Equal("Мария", loaded[0].FirstName);
            Assert.Equal("Алексей", loaded[1].FirstName);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}