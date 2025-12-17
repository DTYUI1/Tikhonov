using System;
using System.Linq;
using Xunit;

public class MyListTests
{
    [Fact]
    public void Add_And_Indexer_WorkCorrectly()
    {
        var list = new MyList<int>();

        list.Add(10);
        list.Add(20);
        list.Add(30);

        Assert.Equal(3, list.Count);
        Assert.Equal(10, list[0]);
        Assert.Equal(20, list[1]);
        Assert.Equal(30, list[2]);
    }

    [Fact]
    public void Indexer_Get_InvalidIndex_Throws()
    {
        var list = new MyList<int>();
        list.Add(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = list[-1]; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = list[1]; });
    }

    [Fact]
    public void Indexer_Set_InvalidIndex_Throws()
    {
        var list = new MyList<int>();
        list.Add(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => list[-1] = 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => list[1] = 5);
    }

    [Fact]
    public void Insert_InsertsElementAtCorrectPosition()
    {
        var list = new MyList<int>();
        list.Add(1);
        list.Add(3);

        // в середину
        list.Insert(1, 2);
        Assert.Equal(new[] { 1, 2, 3 }, list.ToArray());

        // в начало
        list.Insert(0, 0);
        Assert.Equal(new[] { 0, 1, 2, 3 }, list.ToArray());

        // в конец (index == Count)
        list.Insert(list.Count, 4);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, list.ToArray());
    }

    [Fact]
    public void Insert_InvalidIndex_Throws()
    {
        var list = new MyList<int>();
        list.Add(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(2, 10)); // Count == 1
    }

    [Fact]
    public void Remove_RemovesFirstOccurence()
    {
        var list = new MyList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(2);
        list.Add(3);

        bool removed = list.Remove(2);

        Assert.True(removed);
        Assert.Equal(new[] { 1, 2, 3 }, list.ToArray());
    }

    [Fact]
    public void Remove_NonExisting_ReturnsFalse()
    {
        var list = new MyList<int> { 1, 2, 3 };

        bool removed = list.Remove(5);

        Assert.False(removed);
        Assert.Equal(new[] { 1, 2, 3 }, list.ToArray());
    }

    [Fact]
    public void RemoveAt_RemovesByIndex()
    {
        var list = new MyList<int> { 1, 2, 3 };

        list.RemoveAt(1);

        Assert.Equal(new[] { 1, 3 }, list.ToArray());
    }

    [Fact]
    public void RemoveAt_InvalidIndex_Throws()
    {
        var list = new MyList<int> { 1 };

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
    }

    [Fact]
    public void Contains_And_IndexOf_WorkCorrectly()
    {
        var list = new MyList<string>();
        list.Add("a");
        list.Add("b");

        Assert.True(list.Contains("a"));
        Assert.False(list.Contains("x"));
        Assert.Equal(0, list.IndexOf("a"));
        Assert.Equal(-1, list.IndexOf("x"));
    }

    [Fact]
    public void CopyTo_CopiesElements()
    {
        var list = new MyList<int> { 1, 2, 3 };
        var array = new int[5];

        list.CopyTo(array, 1);

        Assert.Equal(0, array[0]);
        Assert.Equal(1, array[1]);
        Assert.Equal(2, array[2]);
        Assert.Equal(3, array[3]);
        Assert.Equal(0, array[4]);
    }

    [Fact]
    public void CopyTo_InvalidArguments_Throw()
    {
        var list = new MyList<int> { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[3], -1));
        Assert.Throws<ArgumentException>(() => list.CopyTo(new int[3], 1)); // 3 элементов не помещаются c index=1
    }

    [Fact]
    public void Clear_RemovesAllElements()
    {
        var list = new MyList<int> { 1, 2, 3 };

        list.Clear();

        Assert.Equal(0, list.Count);
        Assert.Empty(list.ToArray());
    }

    [Fact]
    public void Add_MoreThanDefaultCapacity_Works()
    {
        var list = new MyList<int>();

        for (int i = 0; i < 20; i++)
            list.Add(i);

        Assert.Equal(20, list.Count);
        Assert.Equal(Enumerable.Range(0, 20), list.ToArray());
    }
}