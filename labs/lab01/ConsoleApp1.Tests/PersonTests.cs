using System;
using Xunit;

public class PersonTests
{
    [Fact]
    public void FullName_ВозвращаетИмяИФамилию()
    {
        var person = new Person
        {
            FirstName = "Иван",
            LastName = "Иванов"
        };

        Assert.Equal("Иван Иванов", person.FullName);
    }

    [Theory]
    [InlineData(17, false)]
    [InlineData(18, true)]
    [InlineData(30, true)]
    public void IsAdult_КорректноОпределяетСовершеннолетие(int age, bool expected)
    {
        var person = new Person { Age = age };

        Assert.Equal(expected, person.IsAdult);
    }

    [Fact]
    public void Email_СохраняетКорректныйАдрес()
    {
        var person = new Person();

        person.Email = "test@example.com";

        Assert.Equal("test@example.com", person.Email);
    }

    [Fact]
    public void Email_БросаетИсключениеПриНекорректномАдресе()
    {
        var person = new Person();

        var ex = Assert.Throws<ArgumentException>(() => person.Email = "invalid-email");

        Assert.Contains("Email должен содержать @", ex.Message);
    }
}