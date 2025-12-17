using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using CollectionsPerf;   // наш консольный проект

public class CollectionPerformanceTests
{
    [Fact]
    public void MeasureTime_ВозвращаетНеотрицательноеВремя()
    {
        var method = typeof(Program).GetMethod(
            "MeasureTime",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        Action work = () =>
        {
            var list = new List<int>();
            for (int i = 0; i < 5000; i++)
                list.Add(i);
        };

        var result = (TimeSpan)method!.Invoke(null, new object[] { work })!;

        Assert.True(result >= TimeSpan.Zero);
    }

    [Fact]
    public void GetMiddleNode_КорректноИщетСередину()
    {
        var method = typeof(Program).GetMethod(
            "GetMiddleNode",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Пустой список -> null
        var empty = new LinkedList<int>();
        var midEmpty = (LinkedListNode<int>?)method!.Invoke(null, new object[] { empty });
        Assert.Null(midEmpty);

        // Один элемент
        var one = new LinkedList<int>(new[] { 10 });
        var midOne = (LinkedListNode<int>)method.Invoke(null, new object[] { one })!;
        Assert.Equal(10, midOne.Value);

        // Нечётное количество: 1..5 -> середина 3
        var five = new LinkedList<int>(Enumerable.Range(1, 5));
        var midFive = (LinkedListNode<int>)method.Invoke(null, new object[] { five })!;
        Assert.Equal(3, midFive.Value);

        // Чётное: 1..4 -> алгоритм даёт "левую" середину = 2
        var four = new LinkedList<int>(Enumerable.Range(1, 4));
        var midFour = (LinkedListNode<int>)method.Invoke(null, new object[] { four })!;
        Assert.Equal(2, midFour.Value);
    }

    [Fact]
    public void TestList_ОтрабатываетБезИсключений()
    {
        InvokePrivateTestMethod("TestList", elementsCount: 1000, repeatTests: 2);
    }

    [Fact]
    public void TestLinkedList_ОтрабатываетБезИсключений()
    {
        InvokePrivateTestMethod("TestLinkedList", elementsCount: 1000, repeatTests: 2);
    }

    [Fact]
    public void TestQueue_ОтрабатываетБезИсключений()
    {
        InvokePrivateTestMethod("TestQueue", elementsCount: 1000, repeatTests: 2);
    }

    [Fact]
    public void TestStack_ОтрабатываетБезИсключений()
    {
        InvokePrivateTestMethod("TestStack", elementsCount: 1000, repeatTests: 2);
    }

    [Fact]
    public void TestImmutableList_ОтрабатываетБезИсключений()
    {
        // ImmutableList работает медленнее, поэтому чуть меньше данных
        InvokePrivateTestMethod("TestImmutableList", elementsCount: 500, repeatTests: 1);
    }

    private static void InvokePrivateTestMethod(string name, int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Если метод бросит исключение — тест упадёт
        method!.Invoke(null, new object[] { elementsCount, repeatTests });
    }
}