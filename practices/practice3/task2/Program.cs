using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Введите данные: ");

        string s = Console.ReadLine();

        string res = "";

        int startb = 0;
        int endb = 0;


        //Дана строка с любыми вложенными скобками: "a(bc(de)f)g(h(i))"
        //Нужно удалить только содержимое скобок, сами скобки тоже убрать: Вывод: "ag"
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '(')
            {
                startb += 1;
            }
            else if (s[i] == ')')
            {

                endb += 1;
                if (endb > startb)
                {
                    Console.WriteLine("Ошибка: лишняя закрывающая скобка!");
                    return;
                }
            }
            else
            {
                if (startb == endb)
                {
                    res += s[i];
                }
            }
        }


        if (startb != endb)
        {
            Console.WriteLine($"Ошибка: не все скобки закрыты! startb={startb}, endb={endb}");
        }
        else
        {
            Console.WriteLine(res);
        }
    }
}