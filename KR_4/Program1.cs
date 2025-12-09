// See https://aka.ms/new-console-template for more information
using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        Console.WriteLine("");
        int data = 0;

        //object smth = typeof(data);

        Type type1 = typeof(int);

        GetTypeInfo(type1);
    }

    static void GetTypeInfo(Type type) 
    {

        Console.WriteLine("Имя типа");
        Console.WriteLine(type.Name);
        //Console.WriteLine(type1.Name);
        Console.WriteLine("---------------Свойства и их типы-------------");
        foreach (var t in type.GetProperties())
        {
            Console.WriteLine(t.Name);
            Console.WriteLine(t.GetType().Name);
        }
        Console.WriteLine("---------------Методы типа-------------");
        foreach (var d in type.GetMethods())
        {
            Console.WriteLine(d.Name);
        }

    }
}