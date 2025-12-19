using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ProducerConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Производитель-Потребитель");
            Console.WriteLine();
            
            Console.WriteLine("Решение с BlockingCollection:");
            RunWithBlockingCollection();
            
            Thread.Sleep(2000);
            
            Console.WriteLine();
            Console.WriteLine("Решение с SemaphoreSlim и lock:");
            RunWithSemaphore();
        }

        public static void RunWithBlockingCollection()
        {
            const int BUFFER_SIZE = 5;
            const int PRODUCERS_COUNT = 2;
            const int CONSUMERS_COUNT = 2;
            const int ITEMS_PER_PRODUCER = 10;

            BlockingCollection<int> buffer = new BlockingCollection<int>(BUFFER_SIZE);
            
            Thread[] producers = new Thread[PRODUCERS_COUNT];
            for (int i = 0; i < PRODUCERS_COUNT; i++)
            {
                int producerId = i;
                producers[i] = new Thread(() => ProducerWithBlockingCollection(producerId, buffer, ITEMS_PER_PRODUCER));
                producers[i].Start();
            }

            Thread[] consumers = new Thread[CONSUMERS_COUNT];
            for (int i = 0; i < CONSUMERS_COUNT; i++)
            {
                int consumerId = i;
                consumers[i] = new Thread(() => ConsumerWithBlockingCollection(consumerId, buffer, ITEMS_PER_PRODUCER * PRODUCERS_COUNT / CONSUMERS_COUNT));
                consumers[i].Start();
            }

            foreach (var producer in producers)
            {
                producer.Join();
            }
            
            buffer.CompleteAdding();
            
            foreach (var consumer in consumers)
            {
                consumer.Join();
            }
            
            Console.WriteLine("Все операции завершены");
        }

        public static void RunWithSemaphore()
        {
            const int BUFFER_SIZE = 5;
            const int PRODUCERS_COUNT = 2;
            const int CONSUMERS_COUNT = 2;
            const int ITEMS_PER_PRODUCER = 10;

            int[] buffer = new int[BUFFER_SIZE];
            int count = 0, inIndex = 0, outIndex = 0;
            object bufferLock = new object();
            SemaphoreSlim empty = new SemaphoreSlim(BUFFER_SIZE, BUFFER_SIZE);
            SemaphoreSlim full = new SemaphoreSlim(0, BUFFER_SIZE);
            bool productionComplete = false;
            
            Thread[] producers = new Thread[PRODUCERS_COUNT];
            for (int i = 0; i < PRODUCERS_COUNT; i++)
            {
                int producerId = i;
                producers[i] = new Thread(() => ProducerWithSemaphore(producerId, buffer, bufferLock, 
                    empty, full, ref count, ref inIndex, BUFFER_SIZE, ITEMS_PER_PRODUCER));
                producers[i].Start();
            }

            Thread[] consumers = new Thread[CONSUMERS_COUNT];
            for (int i = 0; i < CONSUMERS_COUNT; i++)
            {
                int consumerId = i;
                consumers[i] = new Thread(() => ConsumerWithSemaphore(consumerId, buffer, bufferLock, 
                    empty, full, ref count, ref outIndex, BUFFER_SIZE, ref productionComplete, 
                    ITEMS_PER_PRODUCER * PRODUCERS_COUNT / CONSUMERS_COUNT));
                consumers[i].Start();
            }

            foreach (var producer in producers)
            {
                producer.Join();
            }
            
            lock (bufferLock)
            {
                productionComplete = true;
            }
            
            for (int i = 0; i < CONSUMERS_COUNT; i++)
            {
                full.Release();
            }
            
            foreach (var consumer in consumers)
            {
                consumer.Join();
            }
            
            Console.WriteLine("Все операции завершены");
        }

        public static void ProducerWithBlockingCollection(int id, BlockingCollection<int> buffer, int itemsCount)
        {
            for (int i = 0; i < itemsCount; i++)
            {
                int item = id * 100 + i;
                Thread.Sleep(new Random().Next(100, 300));
                
                buffer.Add(item);
                Console.WriteLine($"Производитель {id} произвел: {item}. В буфере: {buffer.Count}");
            }
            
            Console.WriteLine($"Производитель {id} завершил работу");
        }

        public static void ConsumerWithBlockingCollection(int id, BlockingCollection<int> buffer, int itemsCount)
        {
            for (int i = 0; i < itemsCount; i++)
            {
                if (buffer.TryTake(out int item, 1000))
                {
                    Thread.Sleep(new Random().Next(200, 400));
                    Console.WriteLine($"Потребитель {id} потребил: {item}. В буфере: {buffer.Count}");
                }
            }
            
            Console.WriteLine($"Потребитель {id} завершил работу");
        }

        public static void ProducerWithSemaphore(int id, int[] buffer, object bufferLock, 
            SemaphoreSlim empty, SemaphoreSlim full, ref int count, ref int inIndex, 
            int bufferSize, int itemsCount)
        {
            for (int i = 0; i < itemsCount; i++)
            {
                int item = id * 100 + i;
                Thread.Sleep(new Random().Next(100, 300));
                
                empty.Wait();
                
                lock (bufferLock)
                {
                    buffer[inIndex] = item;
                    inIndex = (inIndex + 1) % bufferSize;
                    count++;
                    
                    Console.WriteLine($"Производитель {id} произвел: {item}. В буфере: {count}");
                }
                
                full.Release();
            }
            
            Console.WriteLine($"Производитель {id} завершил работу");
        }

        public static void ConsumerWithSemaphore(int id, int[] buffer, object bufferLock,
            SemaphoreSlim empty, SemaphoreSlim full, ref int count, ref int outIndex,
            int bufferSize, ref bool productionComplete, int itemsCount)
        {
            int consumed = 0;
            
            while (consumed < itemsCount)
            {
                bool success = full.Wait(1000);
                
                lock (bufferLock)
                {
                    if (success && count > 0)
                    {
                        int item = buffer[outIndex];
                        outIndex = (outIndex + 1) % bufferSize;
                        count--;
                        
                        empty.Release();
                        
                        Thread.Sleep(new Random().Next(200, 400));
                        Console.WriteLine($"Потребитель {id} потребил: {item}. В буфере: {count}");
                        consumed++;
                    }
                    else if (productionComplete && count == 0)
                    {
                        break;
                    }
                }
            }
            
            Console.WriteLine($"Потребитель {id} завершил работу. Потребил элементов: {consumed}");
        }
    }
}