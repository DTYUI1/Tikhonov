using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Лабораторная работа: Анализ производительности коллекций ===\n");

        int elementsCount = 100000;
        int repeatTests = 5;

        Console.WriteLine("1. Тестирование List<int>:");
        TestList(elementsCount, repeatTests);

        Console.WriteLine("\n2. Тестирование LinkedList<int>:");
        TestLinkedList(elementsCount, repeatTests);

        Console.WriteLine("\n3. Тестирование Queue<int>:");
        TestQueue(elementsCount, repeatTests);

        Console.WriteLine("\n4. Тестирование Stack<int>:");
        TestStack(elementsCount, repeatTests);

        Console.WriteLine("\n5. Тестирование ImmutableList<int>:");
        TestImmutableList(elementsCount, repeatTests);

        Console.WriteLine("\n=== Тестирование завершено ===");
    }

    // ========== List<int> ==========
    static void TestList(int elementsCount, int repeatTests)
    {
        var addTimes = new List<TimeSpan>();
        var addFirstTimes = new List<TimeSpan>();
        var insertMiddleTimes = new List<TimeSpan>();
        var removeTimes = new List<TimeSpan>();
        var removeFirstTimes = new List<TimeSpan>();
        var removeMiddleTimes = new List<TimeSpan>();
        var searchTimes = new List<TimeSpan>();
        var getByIndexTimes = new List<TimeSpan>();

        for (int test = 0; test < repeatTests; test++)
        {
            addTimes.Add(MeasureTime(() => {
                var list = new List<int>();
                for (int i = 0; i < elementsCount; i++) list.Add(i);
            }));

            addFirstTimes.Add(MeasureTime(() => {
                var list = new List<int>();
                for (int i = 0; i < elementsCount; i++) list.Insert(0, i);
            }));

            insertMiddleTimes.Add(MeasureTime(() => {
                var list = new List<int>();
                for (int i = 0; i < elementsCount; i++)
                {
                    int middleIndex = list.Count / 2;
                    list.Insert(middleIndex, i);
                }
            }));

            var listForRemove = new List<int>();
            for (int i = 0; i < elementsCount; i++) listForRemove.Add(i);
            removeTimes.Add(MeasureTime(() => {
                var list = new List<int>(listForRemove);
                while (list.Count > 0) list.RemoveAt(list.Count - 1);
            }));

            removeFirstTimes.Add(MeasureTime(() => {
                var list = new List<int>(listForRemove);
                while (list.Count > 0) list.RemoveAt(0);
            }));

            removeMiddleTimes.Add(MeasureTime(() => {
                var list = new List<int>(listForRemove);
                while (list.Count > 0)
                {
                    int middleIndex = list.Count / 2;
                    list.RemoveAt(middleIndex);
                }
            }));

            var listForSearch = new List<int>();
            for (int i = 0; i < elementsCount; i++) listForSearch.Add(i);
            searchTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    listForSearch.Contains(i % elementsCount);
                }
            }));

            getByIndexTimes.Add(MeasureTime(() => {
                var list = new List<int>(listForRemove);
                for (int i = 0; i < elementsCount; i++)
                {
                    int index = i % Math.Max(1, list.Count);
                    _ = list[index];
                }
            }));
        }

        PrintResults("Добавление в конец (100000)", addTimes);
        PrintResults("Добавление в начало (100000)", addFirstTimes);
        PrintResults("Вставка в середину (100000)", insertMiddleTimes);
        PrintResults("Удаление с конца (100000)", removeTimes);
        PrintResults("Удаление с начала (100000)", removeFirstTimes);
        PrintResults("Удаление из середины (100000)", removeMiddleTimes);
        PrintResults("Поиск элемента (100000 раз)", searchTimes);
        PrintResults("Получение по индексу (100000 раз)", getByIndexTimes);
    }

    // ========== LinkedList<int> ==========
    static void TestLinkedList(int elementsCount, int repeatTests)
    {
        var addTimes = new List<TimeSpan>();
        var addFirstTimes = new List<TimeSpan>();
        var insertMiddleTimes = new List<TimeSpan>();
        var removeTimes = new List<TimeSpan>();
        var removeFirstTimes = new List<TimeSpan>();
        var removeMiddleTimes = new List<TimeSpan>();
        var searchTimes = new List<TimeSpan>();

        for (int test = 0; test < repeatTests; test++)
        {
            addTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>();
                for (int i = 0; i < elementsCount; i++) linkedList.AddLast(i);
            }));

            addFirstTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>();
                for (int i = 0; i < elementsCount; i++) linkedList.AddFirst(i);
            }));

            insertMiddleTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>();
                for (int i = 0; i < elementsCount; i++)
                {
                    if (linkedList.Count == 0)
                    {
                        linkedList.AddFirst(i);
                    }
                    else
                    {
                        var middleNode = GetMiddleNode(linkedList);
                        linkedList.AddAfter(middleNode, i);
                    }
                }
            }));

            var linkedListForOperations = new LinkedList<int>();
            for (int i = 0; i < elementsCount; i++) linkedListForOperations.AddLast(i);

            removeTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>(linkedListForOperations);
                while (linkedList.Count > 0) linkedList.RemoveLast();
            }));

            removeFirstTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>(linkedListForOperations);
                while (linkedList.Count > 0) linkedList.RemoveFirst();
            }));

            removeMiddleTimes.Add(MeasureTime(() => {
                var linkedList = new LinkedList<int>(linkedListForOperations);
                for (int i = 0; i < elementsCount && linkedList.Count > 0; i++)
                {
                    var middleNode = GetMiddleNode(linkedList);
                    if (middleNode != null) linkedList.Remove(middleNode);
                }
            }));

            searchTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    linkedListForOperations.Contains(i % elementsCount);
                }
            }));
        }

        PrintResults("Добавление в конец (100000)", addTimes);
        PrintResults("Добавление в начало (100000)", addFirstTimes);
        PrintResults("Вставка в середину (100000)", insertMiddleTimes);
        PrintResults("Удаление с конца (100000)", removeTimes);
        PrintResults("Удаление с начала (100000)", removeFirstTimes);
        PrintResults("Удаление из середины (100000)", removeMiddleTimes);
        PrintResults("Поиск элемента (100000 раз)", searchTimes);
        Console.WriteLine("  Получение по индексу: не поддерживается в LinkedList");
    }

    // ========== Queue<int> ==========
    static void TestQueue(int elementsCount, int repeatTests)
    {
        var enqueueTimes = new List<TimeSpan>();
        var dequeueTimes = new List<TimeSpan>();
        var searchTimes = new List<TimeSpan>();

        for (int test = 0; test < repeatTests; test++)
        {
            enqueueTimes.Add(MeasureTime(() => {
                var queue = new Queue<int>();
                for (int i = 0; i < elementsCount; i++) queue.Enqueue(i);
            }));

            var queueForDequeue = new Queue<int>();
            for (int i = 0; i < elementsCount; i++) queueForDequeue.Enqueue(i);

            dequeueTimes.Add(MeasureTime(() => {
                var queue = new Queue<int>(queueForDequeue);
                while (queue.Count > 0) queue.Dequeue();
            }));

            var queueForSearch = new Queue<int>();
            for (int i = 0; i < elementsCount; i++) queueForSearch.Enqueue(i);

            searchTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    queueForSearch.Contains(i % elementsCount);
                }
            }));
        }

        PrintResults("Добавление (Enqueue, 100000)", enqueueTimes);
        PrintResults("Удаление (Dequeue, 100000)", dequeueTimes);
        PrintResults("Поиск элемента (100000 раз)", searchTimes);
        Console.WriteLine("  Вставка в середину: не поддерживается в Queue");
        Console.WriteLine("  Получение по индексу: не поддерживается в Queue");
    }

    // ========== Stack<int> ==========
    static void TestStack(int elementsCount, int repeatTests)
    {
        var pushTimes = new List<TimeSpan>();
        var popTimes = new List<TimeSpan>();
        var searchTimes = new List<TimeSpan>();

        for (int test = 0; test < repeatTests; test++)
        {
            pushTimes.Add(MeasureTime(() => {
                var stack = new Stack<int>();
                for (int i = 0; i < elementsCount; i++) stack.Push(i);
            }));

            var stackForPop = new Stack<int>();
            for (int i = 0; i < elementsCount; i++) stackForPop.Push(i);

            popTimes.Add(MeasureTime(() => {
                var stack = new Stack<int>(stackForPop);
                while (stack.Count > 0) stack.Pop();
            }));

            var stackForSearch = new Stack<int>();
            for (int i = 0; i < elementsCount; i++) stackForSearch.Push(i);

            searchTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    stackForSearch.Contains(i % elementsCount);
                }
            }));
        }

        PrintResults("Добавление (Push, 100000)", pushTimes);
        PrintResults("Удаление (Pop, 100000)", popTimes);
        PrintResults("Поиск элемента (100000 раз)", searchTimes);
        Console.WriteLine("  Вставка в середину: не поддерживается в Stack");
        Console.WriteLine("  Получение по индексу: не поддерживается в Stack");
    }

    // ========== ImmutableList<int> ==========
    static void TestImmutableList(int elementsCount, int repeatTests)
    {
        var addTimes = new List<TimeSpan>();
        var addFirstTimes = new List<TimeSpan>();
        var insertMiddleTimes = new List<TimeSpan>();
        var removeTimes = new List<TimeSpan>();
        var removeFirstTimes = new List<TimeSpan>();
        var removeMiddleTimes = new List<TimeSpan>();
        var searchTimes = new List<TimeSpan>();
        var getByIndexTimes = new List<TimeSpan>();

        for (int test = 0; test < repeatTests; test++)
        {
            addTimes.Add(MeasureTime(() => {
                var immutableList = ImmutableList<int>.Empty;
                for (int i = 0; i < elementsCount; i++)
                {
                    immutableList = immutableList.Add(i);
                }
            }));

            addFirstTimes.Add(MeasureTime(() => {
                var immutableList = ImmutableList<int>.Empty;
                for (int i = 0; i < elementsCount; i++)
                {
                    immutableList = immutableList.Insert(0, i);
                }
            }));

            insertMiddleTimes.Add(MeasureTime(() => {
                var immutableList = ImmutableList<int>.Empty;
                for (int i = 0; i < elementsCount; i++)
                {
                    int middleIndex = immutableList.Count / 2;
                    immutableList = immutableList.Insert(middleIndex, i);
                }
            }));

            var baseListForRemove = ImmutableList<int>.Empty;
            for (int i = 0; i < elementsCount; i++)
            {
                baseListForRemove = baseListForRemove.Add(i);
            }

            removeTimes.Add(MeasureTime(() => {
                var immutableList = baseListForRemove;
                for (int i = 0; i < elementsCount; i++)
                {
                    if (immutableList.Count > 0)
                        immutableList = immutableList.RemoveAt(immutableList.Count - 1);
                }
            }));

            removeFirstTimes.Add(MeasureTime(() => {
                var immutableList = baseListForRemove;
                for (int i = 0; i < elementsCount; i++)
                {
                    if (immutableList.Count > 0)
                        immutableList = immutableList.RemoveAt(0);
                }
            }));

            removeMiddleTimes.Add(MeasureTime(() => {
                var immutableList = baseListForRemove;
                for (int i = 0; i < elementsCount; i++)
                {
                    if (immutableList.Count > 0)
                    {
                        int middleIndex = immutableList.Count / 2;
                        immutableList = immutableList.RemoveAt(middleIndex);
                    }
                }
            }));

            var baseListForSearch = ImmutableList<int>.Empty;
            for (int i = 0; i < elementsCount; i++)
            {
                baseListForSearch = baseListForSearch.Add(i);
            }

            searchTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    baseListForSearch.Contains(i % elementsCount);
                }
            }));

            getByIndexTimes.Add(MeasureTime(() => {
                for (int i = 0; i < elementsCount; i++)
                {
                    int index = i % Math.Max(1, baseListForSearch.Count);
                    _ = baseListForSearch[index];
                }
            }));
        }

        PrintResults("Добавление в конец (100000)", addTimes);
        PrintResults("Добавление в начало (100000)", addFirstTimes);
        PrintResults("Вставка в середину (100000)", insertMiddleTimes);
        PrintResults("Удаление с конца (100000)", removeTimes);
        PrintResults("Удаление с начала (100000)", removeFirstTimes);
        PrintResults("Удаление из середины (100000)", removeMiddleTimes);
        PrintResults("Поиск элемента (100000 раз)", searchTimes);
        PrintResults("Получение по индексу (100000 раз)", getByIndexTimes);
    }

    
    static TimeSpan MeasureTime(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    static void PrintResults(string operation, List<TimeSpan> times)
    {
        if (times.Count == 0) return;

        var avgMs = times.Average(t => t.TotalMilliseconds);
        var minMs = times.Min(t => t.TotalMilliseconds);
        var maxMs = times.Max(t => t.TotalMilliseconds);

        Console.WriteLine($"  {operation}:");
        Console.WriteLine($"    Среднее: {avgMs:F2} мс");
        Console.WriteLine($"    Минимум: {minMs:F2} мс");
        Console.WriteLine($"    Максимум: {maxMs:F2} мс");
        Console.WriteLine($"    Тестов: {times.Count}");
    }

    static LinkedListNode<int> GetMiddleNode(LinkedList<int> list)
    {
        if (list.Count == 0) return null;

        var slow = list.First;
        var fast = list.First;

        while (fast?.Next != null && fast.Next.Next != null)
        {
            slow = slow.Next;
            fast = fast.Next.Next;
        }

        return slow;
    }
}