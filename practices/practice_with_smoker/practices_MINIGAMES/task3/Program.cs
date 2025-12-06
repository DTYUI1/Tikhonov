using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        List<string> questions = new List<string>();

        using (StreamReader reader = new StreamReader("vic.txt"))
        {
            while (!reader.EndOfStream)
            {
                questions.Add(reader.ReadLine());
            }
        }

        string[][] answers_str = new string[3][];
        answers_str[0] = new string[] { "1. Kirill", "2. Boris", "3. Lize" };
        answers_str[1] = new string[] { "1. Buisnessman", "2. Cat", "3. My dad" };
        answers_str[2] = new string[] { "1. Moscow", "2. Karpo", "3. Antananarivo" };

        List<int> answers = new List<int>() { 1, 1, 3 };
        int correct_answers = 0;

        for (int i = 0; i < answers.Count; i++)
        {
            Console.WriteLine(questions[i]);
            foreach (string ans in answers_str[i])
            {
                Console.WriteLine(ans);
            }

            int choise = Convert.ToInt32(Console.ReadLine());
            if (choise == answers[i]) correct_answers++;
        }

        Console.WriteLine($"Правильных ответов: {correct_answers} из 3");
    }
}