using System;
using System.Threading;

public class MonitorExample
{
    private static readonly object _syncPsmoker = new object();
    private static readonly object _syncMsmoker = new object();
    private static readonly object _syncTsmoker = new object();
    private static readonly object _syncAgent = new object();
    private static bool _smoke = false;
    private static bool _paper = false;
    private static bool _match = false;

    public static void Main()
    {
        Console.WriteLine("Курить вредно!!!");

        // Поток курильщиков 
        Thread PsmokerThread = new Thread(SmokerPapper);
        PsmokerThread.Name = "Курильщик с бумагой";
        PsmokerThread.Start();

        Thread TsmokerThread = new Thread(SmokerTobacco);
        TsmokerThread.Name = "Курильщик с табоком";
        TsmokerThread.Start();

        Thread MsmokerThread = new Thread(SmokerMatches);
        MsmokerThread.Name = "Курильщик со спичками";
        MsmokerThread.Start();

        // Даем курильщикам время начать ожидание
        Thread.Sleep(1000);

        // Поток агентов
        Thread agentThread = new Thread(Agent);
        agentThread.Name = "Агент";
        agentThread.Start();

        agentThread.Join();
    }
    private static void Agent()
    {
        Console.WriteLine($"Агент начал работу");
        lock (_syncAgent)
        {
            for (int i = 0; i < 10; i++)
            {
                // Имитируем подготовку данных
                _match = false; _paper = false; _smoke = false;

                Console.WriteLine("Агент выкладывает вещи на стол");
                Thread.Sleep(1000);

                Random random = new Random();
                int rn = random.Next(1, 3);
                string firts_item = ""; string second_item = "";

                if (rn == 1) { _match = true; _paper = false; firts_item = "спички"; second_item = "бумага"; }
                if (rn == 2) { _match = true; _smoke = false; firts_item = "спички"; second_item = "табако"; }
                if (rn == 3) { _smoke = true; _paper = false; firts_item = "табако"; second_item = "бумага"; }


                Console.WriteLine($"Агент разложил на стол {firts_item} и {second_item}");
                // Уведомляем ожидающий поток

                if (rn == 1)
                {
                    lock (_syncTsmoker)
                    {
                        Monitor.Pulse(_syncTsmoker);
                    }

                }
                else if (rn == 2)
                {
                    lock (_syncPsmoker)
                    {
                        Monitor.Pulse(_syncPsmoker);
                    }
                }
                else
                {
                    lock (_syncMsmoker)
                    {
                        Monitor.Pulse(_syncMsmoker);
                    }
                }

                // Агент ждёт ответа от Курильщика
                Monitor.Wait(_syncAgent);
                Console.WriteLine("Агент дождался курильщика!");

            }

            Console.WriteLine($"Агент завершил работу");

        }

    }


    private static void SmokerPapper()
    {
        Console.WriteLine($"Курильщик с бумагой проснулся");

        lock (_syncPsmoker)
        {
            while (true)
            {
                Console.WriteLine($"Курильщик с бумагой ждет...");

                // Ожидаем, пока данные не будут готовы
                Monitor.Wait(_syncPsmoker);


                Console.WriteLine($"Курильщик с бумагой начал курить ----------");
                Thread.Sleep(2000);
                // Блоагодарим агента и говорим, что покурили
                Console.WriteLine($"Курильщик с бумагой покурил :з");
                lock (_syncAgent)
                {
                    Monitor.Pulse(_syncAgent);
                }

            }
        }
    }

    private static void SmokerTobacco()
    {
        Console.WriteLine($"Курильщик с табако проснулся");

        lock (_syncTsmoker)
        {
            while (true)
            {
                Console.WriteLine($"Курильщик с табако ждет...");

                // Ожидаем, пока данные не будут готовы
                Monitor.Wait(_syncTsmoker);


                Console.WriteLine($"Курильщик с табако начал курить ----------");
                Thread.Sleep(2000);
                // Блоагодарим агента и говорим, что покурили
                Console.WriteLine($"Курильщик с табако покурил :з");
                lock (_syncAgent)
                {
                    Monitor.Pulse(_syncAgent);
                }

            }
        }
    }


    private static void SmokerMatches()
    {
        Console.WriteLine($"Курильщик со спичками проснулся");

        lock (_syncMsmoker)
        {
            while (true)
            {
                Console.WriteLine($"Курильщик со спичками ждет...");

                // Ожидаем, пока данные не будут готовы
                Monitor.Wait(_syncMsmoker);


                Console.WriteLine($"Курильщик со спичками начал курить ----------");
                Thread.Sleep(2000);
                // Блоагодарим агента и говорим, что покурили
                Console.WriteLine($"Курильщик со спичками покурил :з");
                lock (_syncAgent)
                {
                    Monitor.Pulse(_syncAgent);
                }

            }
        }
    }
}