using Xunit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public class CollectionsPerformanceTests
{
    private const int TestElementsCount = 1000; // Используем меньше элементов для быстрых тестов
    private const int RepeatTests = 2;

    [Fact]
    public void TestList_PerformanceTestsCompleteWithoutException()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert (проверяем, что метод выполняется без исключений)
        var exception = Record.Exception(() =>
            program.TestList(TestElementsCount, RepeatTests));

        Assert.Null(exception);
    }

    [Fact]
    public void TestLinkedList_PerformanceTestsCompleteWithoutException()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert
        var exception = Record.Exception(() =>
            program.TestLinkedList(TestElementsCount, RepeatTests));

        Assert.Null(exception);
    }

    [Fact]
    public void TestQueue_PerformanceTestsCompleteWithoutException()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert
        var exception = Record.Exception(() =>
            program.TestQueue(TestElementsCount, RepeatTests));

        Assert.Null(exception);
    }

    [Fact]
    public void TestStack_PerformanceTestsCompleteWithoutException()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert
        var exception = Record.Exception(() =>
            program.TestStack(TestElementsCount, RepeatTests));

        Assert.Null(exception);
    }

    [Fact]
    public void TestImmutableList_PerformanceTestsCompleteWithoutException()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert
        var exception = Record.Exception(() =>
            program.TestImmutableList(TestElementsCount, RepeatTests));

        Assert.Null(exception);
    }

    [Fact]
    public void MeasureTime_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act
        var time = program.MeasureTime(() =>
        {
            for (int i = 0; i < 1000; i++) { }
        });

        // Assert
        Assert.True(time > TimeSpan.Zero);
    }

    [Fact]
    public void GetMiddleNode_WithEmptyList_ReturnsNull()
    {
        // Arrange
        var program = new ProgramWrapper();
        var list = new LinkedList<int>();

        // Act
        var result = program.GetMiddleNode(list);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMiddleNode_WithSingleElement_ReturnsFirst()
    {
        // Arrange
        var program = new ProgramWrapper();
        var list = new LinkedList<int>();
        list.AddLast(42);

        // Act
        var result = program.GetMiddleNode(list);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GetMiddleNode_WithMultipleElements_ReturnsCorrectNode()
    {
        // Arrange
        var program = new ProgramWrapper();
        var list = new LinkedList<int>();

        // Добавляем 5 элементов
        for (int i = 1; i <= 5; i++)
            list.AddLast(i * 10);

        // Act
        var result = program.GetMiddleNode(list);

        // Assert
        Assert.NotNull(result);
        // Для 5 элементов середина должна быть 3-й элемент (индекс 2)
        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void PrintResults_WithValidData_DoesNotThrow()
    {
        // Arrange
        var program = new ProgramWrapper();
        var times = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(150)
        };

        // Act & Assert
        var exception = Record.Exception(() =>
            program.PrintResults("Test Operation", times));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void TestList_WithDifferentSizes_CompletesWithoutException(int size)
    {
        // Arrange
        var program = new ProgramWrapper();

        // Act & Assert
        var exception = Record.Exception(() =>
            program.TestList(size, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void TestList_AddOperation_ActuallyAddsElements()
    {
        // Arrange
        var program = new ProgramWrapper();
        int testSize = 100;
        var list = new List<int>();

        // Act
        program.MeasureTime(() =>
        {
            for (int i = 0; i < testSize; i++)
                list.Add(i);
        });

        // Assert
        Assert.Equal(testSize, list.Count);
        Assert.Equal(testSize - 1, list.Last());
    }
}

// Класс-обертка для доступа к private методам Program
public class ProgramWrapper
{
    public void TestList(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestList",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }

    public void TestLinkedList(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestLinkedList",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }

    public void TestQueue(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestQueue",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }

    public void TestStack(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestStack",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }

    public void TestImmutableList(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestImmutableList",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }

    public TimeSpan MeasureTime(Action action)
    {
        var method = typeof(Program).GetMethod("MeasureTime",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (TimeSpan)method?.Invoke(null, new object[] { action });
    }

    public void PrintResults(string operation, List<TimeSpan> times)
    {
        var method = typeof(Program).GetMethod("PrintResults",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { operation, times });
    }

    public LinkedListNode<int> GetMiddleNode(LinkedList<int> list)
    {
        var method = typeof(Program).GetMethod("GetMiddleNode",
            BindingFlags.NonPublic | BindingFlags.Static);
        return (LinkedListNode<int>)method?.Invoke(null, new object[] { list });
    }
}