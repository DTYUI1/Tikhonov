using System;
using System.Threading;

namespace DiningPhilosophers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Обедающие философы");
            Console.WriteLine();
            
            Console.WriteLine("Версия с deadlock:");
            RunWithDeadlock();
            
            Thread.Sleep(2000);
            
            Console.WriteLine();
            Console.WriteLine("Версия без deadlock:");
            RunWithoutDeadlock();
        }

        public static void RunWithDeadlock()
        {
            const int PHILOSOPHERS_COUNT = 5;
            object[] forks = new object[PHILOSOPHERS_COUNT];
            Thread[] philosophers = new Thread[PHILOSOPHERS_COUNT];

            for (int i = 0; i < PHILOSOPHERS_COUNT; i++)
            {
                forks[i] = new object();
            }

            for (int i = 0; i < PHILOSOPHERS_COUNT; i++)
            {
                int id = i;
                philosophers[i] = new Thread(() => PhilosopherWithDeadlock(id, forks));
                philosophers[i].Start();
            }

            Thread.Sleep(3000);
            
            foreach (var philosopher in philosophers)
            {
                philosopher.Interrupt();
            }
        }

        public static void RunWithoutDeadlock()
        {
            const int PHILOSOPHERS_COUNT = 5;
            object[] forks = new object[PHILOSOPHERS_COUNT];
            Thread[] philosophers = new Thread[PHILOSOPHERS_COUNT];
            SemaphoreSlim table = new SemaphoreSlim(PHILOSOPHERS_COUNT - 1, PHILOSOPHERS_COUNT - 1);

            for (int i = 0; i < PHILOSOPHERS_COUNT; i++)
            {
                forks[i] = new object();
            }

            for (int i = 0; i < PHILOSOPHERS_COUNT; i++)
            {
                int id = i;
                philosophers[i] = new Thread(() => PhilosopherWithoutDeadlock(id, forks, table));
                philosophers[i].Start();
            }

            Thread.Sleep(5000);
            
            foreach (var philosopher in philosophers)
            {
                philosopher.Interrupt();
            }
        }

        public static void PhilosopherWithDeadlock(int id, object[] forks)
        {
            int leftFork = id;
            int rightFork = (id + 1) % forks.Length;

            try
            {
                while (true)
                {
                    Console.WriteLine($"Философ {id} думает");
                    Thread.Sleep(new Random().Next(500, 1000));

                    Console.WriteLine($"Философ {id} хочет взять левую вилку {leftFork}");
                    lock (forks[leftFork])
                    {
                        Console.WriteLine($"Философ {id} взял левую вилку {leftFork}");
                        Thread.Sleep(100);

                        Console.WriteLine($"Философ {id} хочет взять правую вилку {rightFork}");
                        lock (forks[rightFork])
                        {
                            Console.WriteLine($"Философ {id} взял правую вилку {rightFork}");
                            
                            Console.WriteLine($"Философ {id} ест с вилками {leftFork} и {rightFork}");
                            Thread.Sleep(new Random().Next(500, 1000));
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine($"Философ {id} завершил трапезу");
            }
        }

        public static void PhilosopherWithoutDeadlock(int id, object[] forks, SemaphoreSlim table)
        {
            int leftFork = id;
            int rightFork = (id + 1) % forks.Length;

            if (id == forks.Length - 1)
            {
                leftFork = (id + 1) % forks.Length;
                rightFork = id;
            }

            try
            {
                while (true)
                {
                    Console.WriteLine($"Философ {id} думает");
                    Thread.Sleep(new Random().Next(500, 1000));

                    table.Wait();
                    
                    Console.WriteLine($"Философ {id} хочет взять левую вилку {leftFork}");
                    lock (forks[leftFork])
                    {
                        Console.WriteLine($"Философ {id} взял левую вилку {leftFork}");
                        
                        Console.WriteLine($"Философ {id} хочет взять правую вилку {rightFork}");
                        lock (forks[rightFork])
                        {
                            Console.WriteLine($"Философ {id} взял правую вилку {rightFork}");
                            
                            Console.WriteLine($"Философ {id} ест с вилками {leftFork} и {rightFork}");
                            Thread.Sleep(new Random().Next(500, 1000));
                        }
                    }
                    
                    table.Release();
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine($"Философ {id} завершил трапезу");
            }
        }
    }
}