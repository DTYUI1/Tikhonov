

using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        int N = Convert.ToInt32(Console.ReadLine());
        Thread thread = new Thread(() =>
        {
            for (int i = 1; i <= N; i++)
            {
                Console.WriteLine(i);
            }
        });


        thread.Start();
        thread.Join();
    }

}