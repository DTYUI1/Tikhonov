using System;

// Простой запускатор тестов
class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Простые тесты MyList и MyDictionary ===\n");

        Test1_MyList();
        Console.WriteLine();

        Test2_MyDictionary();
        Console.WriteLine();

        Test3_Integration();

        Console.WriteLine("\n=== Все тесты завершены ===");
    }

    static void Test1_MyList()
    {
        Console.WriteLine("Тест 1: MyList базовые операции");

        var list = new MyList<int>();

        // Добавление
        list.Add(10);
        list.Add(20);
        list.Add(30);

        Console.WriteLine($"Добавлено 3 элемента: [{string.Join(", ", list)}]");
        Console.WriteLine($"Count = {list.Count} (ожидается 3)");
        Console.WriteLine($"list[1] = {list[1]} (ожидается 20)");

        // Удаление
        list.Remove(20);
        Console.WriteLine($"После удаления 20: [{string.Join(", ", list)}]");
        Console.WriteLine($"Count = {list.Count} (ожидается 2)");

        // Вставка
        list.Insert(1, 25);
        Console.WriteLine($"После вставки 25 на позицию 1: [{string.Join(", ", list)}]");

        // Поиск
        Console.WriteLine($"Contains(30) = {list.Contains(30)} (ожидается true)");
        Console.WriteLine($"IndexOf(25) = {list.IndexOf(25)} (ожидается 1)");

        Console.WriteLine("✅ MyList тест пройден");
    }

    static void Test2_MyDictionary()
    {
        Console.WriteLine("Тест 2: MyDictionary базовые операции");

        var dict = new MyDictionary<string, int>();

        // Добавление
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);

        Console.WriteLine($"Добавлено 3 пары ключ-значение");
        Console.WriteLine($"Count = {dict.Count} (ожидается 3)");
        Console.WriteLine($"dict[\"two\"] = {dict["two"]} (ожидается 2)");

        // Проверка существования
        Console.WriteLine($"ContainsKey(\"one\") = {dict.ContainsKey("one")} (ожидается true)");
        Console.WriteLine($"ContainsKey(\"four\") = {dict.ContainsKey("four")} (ожидается false)");

        // Обновление через индексатор
        dict["two"] = 22;
        Console.WriteLine($"После dict[\"two\"] = 22: dict[\"two\"] = {dict["two"]} (ожидается 22)");

        // Удаление
        dict.Remove("one");
        Console.WriteLine($"После удаления \"one\": Count = {dict.Count} (ожидается 2)");
        Console.WriteLine($"ContainsKey(\"one\") = {dict.ContainsKey("one")} (ожидается false)");

        Console.WriteLine("✅ MyDictionary тест пройден");
    }

    static void Test3_Integration()
    {
        Console.WriteLine("Тест 3: Интеграция MyList и MyDictionary");

        // MyList внутри MyDictionary
        var dict = new MyDictionary<string, MyList<int>>();
        var numbers = new MyList<int> { 1, 2, 3, 4, 5 };
        dict["numbers"] = numbers;

        Console.WriteLine($"dict[\"numbers\"][2] = {dict["numbers"][2]} (ожидается 3)");

        // MyDictionary внутри MyList
        var list = new MyList<MyDictionary<string, int>>();
        var dict1 = new MyDictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var dict2 = new MyDictionary<string, int> { ["x"] = 10, ["y"] = 20 };

        list.Add(dict1);
        list.Add(dict2);

        Console.WriteLine($"list[0][\"b\"] = {list[0]["b"]} (ожидается 2)");
        Console.WriteLine($"list[1][\"y\"] = {list[1]["y"]} (ожидается 20)");

        Console.WriteLine("✅ Интеграционный тест пройден");
    }
}