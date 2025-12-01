using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Создание и заполнение коллекций
        var sortedList = new SortedList<int, string>();
        var sortedDictionary = new SortedDictionary<int, string>();

        // Добавление элементов в SortedList
        sortedList.Add(3, "Alice");
        sortedList.Add(1, "Bob");
        sortedList.Add(5, "Charlie");

        // Добавление элементов в SortedDictionary
        sortedDictionary.Add(3, "Alice");
        sortedDictionary.Add(1, "Bob");
        sortedDictionary.Add(5, "Charlie");

        // Вывод содержимого SortedList
        Console.WriteLine("SortedList:");
        foreach (var item in sortedList)
            Console.WriteLine($"{item.Key}: {item.Value}");

        // Вывод содержимого SortedDictionary
        Console.WriteLine("\nSortedDictionary:");
        foreach (var item in sortedDictionary)
            Console.WriteLine($"{item.Key}: {item.Value}");

        // Тестирование производительности
        Console.WriteLine("\nСкорость добавления 10000 элементов:");

        // Тест SortedList
        var stopwatch = Stopwatch.StartNew();
        var testList = new SortedList<int, string>();
        for (int i = 0; i < 10000; i++)
            testList.Add(i, $"Value{i}");
        Console.WriteLine($"SortedList: {stopwatch.ElapsedMilliseconds} ms");

        // Тест SortedDictionary
        stopwatch.Restart();
        var testDict = new SortedDictionary<int, string>();
        for (int i = 0; i < 10000; i++)
            testDict.Add(i, $"Value{i}");
        Console.WriteLine($"SortedDictionary: {stopwatch.ElapsedMilliseconds} ms");

        // Дополнительные операции
        Console.WriteLine("\nДополнительные операции:");

        // Поиск в SortedList
        stopwatch.Restart();
        bool listContains = sortedList.ContainsKey(3);
        Console.WriteLine($"Поиск в SortedList: {stopwatch.ElapsedTicks} ticks");

        // Поиск в SortedDictionary
        stopwatch.Restart();
        bool dictContains = sortedDictionary.ContainsKey(3);
        Console.WriteLine($"Поиск в SortedDictionary: {stopwatch.ElapsedTicks} ticks");

        // Удаление из SortedList
        stopwatch.Restart();
        sortedList.Remove(3);
        Console.WriteLine($"Удаление из SortedList: {stopwatch.ElapsedTicks} ticks");

        // Удаление из SortedDictionary
        stopwatch.Restart();
        sortedDictionary.Remove(3);
        Console.WriteLine($"Удаление из SortedDictionary: {stopwatch.ElapsedTicks} ticks");
    }
}