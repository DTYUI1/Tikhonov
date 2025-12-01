using System;
using System.Collections.Generic;

class Program
{
    static List<T> FilterCollection<T>(IEnumerable<T> collection, Func<T, bool> condition)
    {
        var result = new List<T>();
        foreach (var item in collection)
        {
            if (condition(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    static bool More30(Product p)
    {
        return p.price > 30;
    }

    static void Main()
    {
        var products = new List<Product>
        {
            new Product("Банан", 50),
            new Product("Хлеб", 30),
            new Product("Жвачка", 20),
            new Product("Картошка 1 кг", 35)
        };

        var expensiveProducts = FilterCollection(products, More30);

        Console.WriteLine("Товары дороже 30 рублей:");
        foreach (var product in expensiveProducts)
        {
            Console.WriteLine($"{product.name} : {product.price} руб.");
        }
    }
}

class Product
{
    public int price { get; set; }
    public string name { get; set; }

    public Product(string name, int price)
    {
        this.price = price;
        this.name = name;
    }
}