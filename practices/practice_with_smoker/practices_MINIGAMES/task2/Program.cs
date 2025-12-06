using System;

class Program
{
    static void Main()
    {
        int points = 0;
        int points_PC = 0;

        while (points < 3 && points_PC < 3)
        {
            int Game_res = gameKMN();
            if (Game_res == 0)
            {
                Console.WriteLine("Вы проиграли в этом раунде!");
                points_PC++;
            }
            else if (Game_res == 1)
            {
                Console.WriteLine("Ничья!");
            }
            else
            {
                Console.WriteLine("Вы победили в этом раунде!");
                points++;
            }

            Console.WriteLine($"Текущий счёт: {points}:{points_PC}");
        }

        if (points > 2)
            Console.WriteLine("Вы МЕГАПОБЕДИТЕЛЬ в КМН!!!");
        else
            Console.WriteLine("Вы проиграли компьютеру :(");
    }

    static int gameKMN()
    {
        Console.WriteLine("Камень, ножницы, бумага?");
        string kmn = Console.ReadLine();
        int n;

        if (kmn.ToLower() == "камень") n = 1;
        else if (kmn.ToLower() == "ножницы") n = 2;
        else if (kmn.ToLower() == "бумага") n = 3;
        else
        {
            Console.WriteLine("Ошибка в вводе! Ну блин выбери кмн..");
            throw new Exception("GG");
        }

        Random random = new Random();
        int rn = random.Next(1, 4);
        string kmn_PC;

        if (rn == 1) kmn_PC = "Камень";
        else if (rn == 2) kmn_PC = "Ножницы";
        else kmn_PC = "Бумага";

        Console.WriteLine($"Компьютер выбрал {kmn_PC}");

        if (rn == n) return 1;

        bool win = false;

        if (n == 1 && rn == 2) win = true;
        else if (n == 1 && rn == 3) win = false;
        else if (n == 2 && rn == 1) win = false;
        else if (n == 2 && rn == 3) win = true;
        else if (n == 3 && rn == 1) win = true;
        else if (n == 3 && rn == 2) win = false;

        return win ? 2 : 0;
    }
}