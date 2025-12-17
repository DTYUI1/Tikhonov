using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class MyDictionaryTests
{
    [Fact]
    public void Add_And_Indexer_Get_WorkCorrectly()
    {
        var dict = new MyDictionary<string, int>();

        dict.Add("one", 1);
        dict.Add("two", 2);

        Assert.Equal(2, dict.Count);
        Assert.True(dict.ContainsKey("one"));
        Assert.True(dict.ContainsKey("two"));
        Assert.False(dict.ContainsKey("three"));

        Assert.Equal(1, dict["one"]);
        Assert.Equal(2, dict["two"]);
    }

    [Fact]
    public void Indexer_Get_MissingKey_ThrowsKeyNotFound()
    {
        var dict = new MyDictionary<string, int>();

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var _ = dict["missing"];
        });
    }

    [Fact]
    public void Add_DuplicateKey_ThrowsArgumentException()
    {
        var dict = new MyDictionary<string, int>();

        dict.Add("key", 1);

        var ex = Assert.Throws<ArgumentException>(() => dict.Add("key", 2));
        Assert.Contains("An element with the same key already exists", ex.Message);
    }

    [Fact]
    public void Indexer_Set_AddsOrUpdates()
    {
        var dict = new MyDictionary<string, int>();

        // добавление нового ключа через индексатор
        dict["a"] = 1;
        Assert.Equal(1, dict.Count);
        Assert.Equal(1, dict["a"]);

        // обновление существующего
        dict["a"] = 10;
        Assert.Equal(1, dict.Count);
        Assert.Equal(10, dict["a"]);
    }

    [Fact]
    public void Remove_ByKey_Works()
    {
        var dict = new MyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };
        int originalCount = dict.Count;

        bool removed = dict.Remove("a");

        Assert.True(removed);
        Assert.Equal(originalCount - 1, dict.Count);
        Assert.False(dict.ContainsKey("a"));

        bool removedAgain = dict.Remove("a");
        Assert.False(removedAgain);
    }

    [Fact]
    public void Remove_ByKeyValuePair_RespectsValue()
    {
        var dict = new MyDictionary<string, int>();
        dict.Add("a", 1);

        // Верное значение -> удаляется
        bool removed = dict.Remove(new KeyValuePair<string, int>("a", 1));
        Assert.True(removed);
        Assert.False(dict.ContainsKey("a"));

        dict.Add("b", 2);
        // Неверное значение -> не удалится
        bool removedWrong = dict.Remove(new KeyValuePair<string, int>("b", 5));
        Assert.False(removedWrong);
        Assert.True(dict.ContainsKey("b"));
    }

    [Fact]
    public void TryGetValue_Works()
    {
        var dict = new MyDictionary<string, int>();
        dict.Add("x", 10);

        bool found = dict.TryGetValue("x", out int value);
        Assert.True(found);
        Assert.Equal(10, value);

        bool notFound = dict.TryGetValue("y", out int value2);
        Assert.False(notFound);
        Assert.Equal(0, value2); // default(int)
    }

    [Fact]
    public void Keys_And_Values_ReturnAllElements()
    {
        var dict = new MyDictionary<string, int>
        {
            ["one"] = 1,
            ["two"] = 2,
            ["three"] = 3
        };

        var keys = dict.Keys;
        var values = dict.Values;

        Assert.Equal(3, keys.Count);
        Assert.Equal(3, values.Count);

        Assert.True(new[] { "one", "two", "three" }.OrderBy(x => x)
            .SequenceEqual(keys.OrderBy(x => x)));

        Assert.True(new[] { 1, 2, 3 }.OrderBy(x => x)
            .SequenceEqual(values.OrderBy(x => x)));
    }

    [Fact]
    public void Contains_KeyValuePair_UsesValueEquality()
    {
        var dict = new MyDictionary<string, int>();
        dict.Add("k", 5);

        Assert.True(dict.Contains(new KeyValuePair<string, int>("k", 5)));
        Assert.False(dict.Contains(new KeyValuePair<string, int>("k", 10)));
        Assert.False(dict.Contains(new KeyValuePair<string, int>("x", 5)));
    }

    [Fact]
    public void CopyTo_CopiesAllPairs()
    {
        var dict = new MyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };

        var array = new KeyValuePair<string, int>[2];
        dict.CopyTo(array, 0);

        Assert.Contains(new KeyValuePair<string, int>("a", 1), array);
        Assert.Contains(new KeyValuePair<string, int>("b", 2), array);
    }

    [Fact]
    public void CopyTo_InvalidArguments_Throw()
    {
        var dict = new MyDictionary<string, int> { ["a"] = 1 };

        Assert.Throws<ArgumentNullException>(() => dict.CopyTo(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => dict.CopyTo(new KeyValuePair<string, int>[1], -1));
        Assert.Throws<ArgumentException>(() =>
            dict.CopyTo(new KeyValuePair<string, int>[0], 0)); // не помещается
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var dict = new MyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };

        dict.Clear();

        Assert.Equal(0, dict.Count);
        Assert.False(dict.ContainsKey("a"));
        Assert.False(dict.ContainsKey("b"));
        Assert.Empty(dict);
    }

    [Fact]
    public void Add_ManyItems_TriggersResize_AndStillWorks()
    {
        var dict = new MyDictionary<int, string>();

        for (int i = 0; i < 100; i++)
            dict.Add(i, $"v{i}");

        Assert.Equal(100, dict.Count);

        for (int i = 0; i < 100; i++)
            Assert.Equal($"v{i}", dict[i]);
    }

    private sealed class BadHash
    {
        public int Id { get; }
        public BadHash(int id) => Id = id;
        public override int GetHashCode() => 1; // все одинаковый хэш
        public override bool Equals(object? obj) =>
            obj is BadHash other && other.Id == Id;
    }

    [Fact]
    public void WorksCorrectlyWithHashCollisions()
    {
        var dict = new MyDictionary<BadHash, string>();

        var keys = Enumerable.Range(0, 10).Select(i => new BadHash(i)).ToArray();
        foreach (var k in keys)
            dict.Add(k, $"v{k.Id}");

        Assert.Equal(10, dict.Count);

        foreach (var k in keys)
            Assert.Equal($"v{k.Id}", dict[k]);
    }
}