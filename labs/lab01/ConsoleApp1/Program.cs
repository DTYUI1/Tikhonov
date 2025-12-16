public class Program
{
    public static async Task Main()
    {
        try
        {
            Console.WriteLine("=== Тестирование Person ===");
            TestPerson();

            Console.WriteLine("\n=== Тестирование PersonSerializer ===");
            await TestPersonSerializerAsync();

            Console.WriteLine("\n=== Тестирование FileResourceManager ===");
            TestFileResourceManager();

            Console.WriteLine("\nВсе тесты завершены успешно!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void TestPerson()
    {
        // Создание объекта Person
        var person = new Person
        {
            FirstName = "Иван",
            LastName = "Иванов",
            Age = 25,
            Password = "secret123",
            Id = "12345",
            BirthDate = new DateTime(1998, 5, 15),
            Email = "ivan@example.com",
            PhoneNumber = "+7-999-123-45-67"
        };

        Console.WriteLine($"FullName: {person.FullName}");
        Console.WriteLine($"IsAdult: {person.IsAdult}");
        Console.WriteLine($"Email: {person.Email}");

        // Проверка валидации email
        try
        {
            person.Email = "invalid-email";
            Console.WriteLine("Валидация email не сработала!");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Валидация email сработала: {ex.Message}");
        }
    }

    private static async Task TestPersonSerializerAsync()
    {
        var serializer = new PersonSerializer();

        // Создание тестового объекта
        var person = new Person
        {
            FirstName = "Мария",
            LastName = "Петрова",
            Age = 30,
            Password = "password123", // Не должно попасть в JSON
            Id = "67890",
            BirthDate = new DateTime(1993, 10, 20),
            Email = "maria@example.com",
            PhoneNumber = "+7-912-345-67-89"
        };

        // Тест 1: Сериализация в строку
        Console.WriteLine("1. Сериализация в строку:");
        string json = serializer.SerializeToJson(person);
        Console.WriteLine(json);

        // Тест 2: Десериализация из строки
        Console.WriteLine("\n2. Десериализация из строки:");
        var deserializedPerson = serializer.DeserializeFromJson(json);
        Console.WriteLine($"Deserialized: {deserializedPerson.FullName}, Email: {deserializedPerson.Email}");

        // Тест 3: Сохранение и загрузка из файла (синхронно)
        Console.WriteLine("\n3. Работа с файлами (синхронно):");
        string filePath = "person.json";
        serializer.SaveToFile(person, filePath);
        Console.WriteLine($"Файл сохранен: {filePath}");

        var loadedPerson = serializer.LoadFromFile(filePath);
        Console.WriteLine($"Загружено: {loadedPerson.FullName}");

        // Тест 4: Асинхронные операции
        Console.WriteLine("\n4. Асинхронные операции:");
        string asyncFilePath = "person_async.json";
        await serializer.SaveToFileAsync(person, asyncFilePath);
        Console.WriteLine($"Файл сохранен асинхронно: {asyncFilePath}");

        var asyncLoadedPerson = await serializer.LoadFromFileAsync(asyncFilePath);
        Console.WriteLine($"Загружено асинхронно: {asyncLoadedPerson.FullName}");

        // Тест 5: Список объектов
        Console.WriteLine("\n5. Работа со списком объектов:");
        var people = new List<Person>
        {
            person,
            new Person { FirstName = "Алексей", LastName = "Сидоров", Age = 22, Email = "alex@example.com" }
        };

        string listFilePath = "people.json";
        serializer.SaveListToFile(people, listFilePath);
        Console.WriteLine($"Список сохранен в: {listFilePath}");

        var loadedPeople = serializer.LoadListFromFile(listFilePath);
        Console.WriteLine($"Загружено {loadedPeople.Count} человек");

        // Очистка тестовых файлов
        CleanupFiles(new[] { filePath, asyncFilePath, listFilePath });
    }

    private static void TestFileResourceManager()
    {
        string testFilePath = "test_file.txt";

        // Тест 1: Запись в файл
        Console.WriteLine("1. Тестирование записи:");
        using (var manager = new FileResourceManager(testFilePath))
        {
            manager.OpenForWriting();
            manager.WriteLine("Первая строка");
            manager.WriteLine("Вторая строка");
            Console.WriteLine("Данные записаны");
        }

        // Тест 2: Чтение из файла
        Console.WriteLine("\n2. Тестирование чтения:");
        using (var manager = new FileResourceManager(testFilePath))
        {
            manager.OpenForReading();
            string content = manager.ReadAllText();
            Console.WriteLine($"Содержимое файла:\n{content}");
        }

        // Тест 3: Добавление текста
        Console.WriteLine("\n3. Тестирование добавления текста:");
        using (var manager = new FileResourceManager(testFilePath))
        {
            manager.AppendText("\nДобавленная строка");
            Console.WriteLine("Текст добавлен");
        }

        // Тест 4: Информация о файле
        Console.WriteLine("\n4. Информация о файле:");
        using (var manager = new FileResourceManager(testFilePath))
        {
            var fileInfo = manager.GetFileInfo();
            Console.WriteLine($"Размер: {fileInfo.Length} байт");
            Console.WriteLine($"Создан: {fileInfo.CreationTime}");
        }

        // Тест 5: Исключения
        Console.WriteLine("\n5. Тестирование исключений:");
        try
        {
            using (var manager = new FileResourceManager("nonexistent.txt"))
            {
                manager.OpenForReading();
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Ожидаемое исключение: {ex.Message}");
        }

        // Очистка
        File.Delete(testFilePath);
    }

    private static void CleanupFiles(string[] files)
    {
        foreach (var file in files)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // Игнорируем ошибки удаления
            }
        }
    }
}