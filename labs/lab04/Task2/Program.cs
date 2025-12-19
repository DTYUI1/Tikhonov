using System;
using System.Collections.Generic;
using System.Threading;

namespace SleepingBarber
{
    public class Program
    {
        private static Queue<int> waitingRoom = new Queue<int>();
        private static readonly object queueLock = new object();
        private static SemaphoreSlim customers = new SemaphoreSlim(0);
        private static SemaphoreSlim barberReady = new SemaphoreSlim(0);
        private static Mutex mutex = new Mutex();
        private static bool barberShopOpen = true;
        private const int MAX_SEATS = 3;

        public static void Main(string[] args)
        {
            Console.WriteLine("Спящий парикмахер");
            
            Thread barberThread = new Thread(Barber);
            barberThread.Start();

            Thread customerGeneratorThread = new Thread(CustomerGenerator);
            customerGeneratorThread.Start();

            Thread.Sleep(10000);
            
            barberShopOpen = false;
            
            customerGeneratorThread.Join();
            
            customers.Release();
            barberThread.Join();
            
            Console.WriteLine();
            Console.WriteLine("Парикмахерская закрыта");
        }

        public static void Barber()
        {
            Console.WriteLine("Парикмахер пришел на работу");
            
            while (barberShopOpen || customers.CurrentCount > 0 || waitingRoom.Count > 0)
            {
                Console.WriteLine("Парикмахер спит");
                customers.Wait();
                
                if (!barberShopOpen && customers.CurrentCount == 0 && waitingRoom.Count == 0)
                    break;

                int customerId;
                lock (queueLock)
                {
                    customerId = waitingRoom.Dequeue();
                }
                
                Console.WriteLine($"Парикмахер начал стричь клиента {customerId}");
                
                Thread.Sleep(new Random().Next(1000, 2000));
                
                Console.WriteLine($"Парикмахер закончил стричь клиента {customerId}");
                
                barberReady.Release();
            }
            
            Console.WriteLine("Парикмахер ушел домой");
        }

        public static void CustomerGenerator()
        {
            int customerId = 1;
            
            while (barberShopOpen)
            {
                Thread.Sleep(new Random().Next(300, 800));
                
                Thread customerThread = new Thread(() => Customer(customerId));
                customerThread.Start();
                customerId++;
            }
            
            Console.WriteLine("Генератор клиентов остановлен");
        }

        public static void Customer(int id)
        {
            mutex.WaitOne();
            
            if (waitingRoom.Count < MAX_SEATS)
            {
                lock (queueLock)
                {
                    waitingRoom.Enqueue(id);
                    Console.WriteLine($"Клиент {id} вошел. Мест в очереди: {waitingRoom.Count}");
                }
                
                customers.Release();
                mutex.ReleaseMutex();
                
                barberReady.Wait();
            }
            else
            {
                Console.WriteLine($"Клиент {id} ушел, нет свободных мест");
                mutex.ReleaseMutex();
            }
        }
    }
}