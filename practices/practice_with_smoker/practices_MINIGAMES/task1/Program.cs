using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Угадайте число (от 1 до 100):");
        int n = Convert.ToInt32(Console.ReadLine());

        Random random = new Random();
        int rn = random.Next(1, 101);

        while (rn != n)
        {
            if (n > rn) Console.WriteLine("Число меньше");
            else if (n < rn) Console.WriteLine("Число больше");
            n = Convert.ToInt32(Console.ReadLine());
        }

        Console.WriteLine("Вы победили!");
    }
}