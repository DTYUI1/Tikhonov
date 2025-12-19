using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Xunit;
using SB = SleepingBarber.Program;

namespace LabThreads.Tests
{
    public class SleepingBarberTests
    {
        private static readonly Type BarberProgramType = typeof(SB);

        /// <summary>
        /// Сбросить статику Program к известному состоянию.
        /// initialWaitingCustomers – сколько клиентов уже сидит в очереди.
        /// barberReadyInitiallySignaled – нужно ли сразу "разбудить" парикмахера,
        /// чтобы клиент не завис на barberReady.Wait().
        /// </summary>
        private static void ResetState(
            int initialWaitingCustomers = 0,
            bool barberReadyInitiallySignaled = false)
        {
            // waitingRoom
            var waitingRoomField = BarberProgramType.GetField(
                "waitingRoom",
                BindingFlags.NonPublic | BindingFlags.Static);
            var queue = new Queue<int>();
            for (int i = 0; i < initialWaitingCustomers; i++)
            {
                queue.Enqueue(i + 1);
            }
            waitingRoomField!.SetValue(null, queue);

            // customers semaphore
            var customersField = BarberProgramType.GetField(
                "customers",
                BindingFlags.NonPublic | BindingFlags.Static);
            customersField!.SetValue(null, new SemaphoreSlim(0));

            // barberReady semaphore
            var barberReadyField = BarberProgramType.GetField(
                "barberReady",
                BindingFlags.NonPublic | BindingFlags.Static);
            int initialCount = barberReadyInitiallySignaled ? 1 : 0;
            barberReadyField!.SetValue(null, new SemaphoreSlim(initialCount, int.MaxValue));

            // mutex
            var mutexField = BarberProgramType.GetField(
                "mutex",
                BindingFlags.NonPublic | BindingFlags.Static);
            mutexField!.SetValue(null, new Mutex());

            // barberShopOpen
            var openField = BarberProgramType.GetField(
                "barberShopOpen",
                BindingFlags.NonPublic | BindingFlags.Static);
            openField!.SetValue(null, true);
        }

        [Fact]
        public void Customer_Leaves_WhenNoSeatsAvailable()
        {
            // MAX_SEATS = 3, заполняем очередь до отказа
            ResetState(initialWaitingCustomers: 3, barberReadyInitiallySignaled: false);

            var originalOut = Console.Out;
            var output = new StringWriter();
            Console.SetOut(output);

            try
            {
                var customerThread = new Thread(() => SB.Customer(99));
                customerThread.Start();

                Assert.True(customerThread.Join(2000),
                    "Поток клиента не завершился за 2 секунды");

                string text = output.ToString();
                Assert.Contains("Клиент 99 ушел, нет свободных мест", text);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Customer_EntersQueue_WhenSeatIsFree()
        {
            // Пустая очередь, barberReady уже "сигнален", чтобы клиент не завис
            ResetState(initialWaitingCustomers: 0, barberReadyInitiallySignaled: true);

            var waitingRoomField = BarberProgramType.GetField(
                "waitingRoom",
                BindingFlags.NonPublic | BindingFlags.Static);

            var originalOut = Console.Out;
            var output = new StringWriter();
            Console.SetOut(output);

            try
            {
                var customerThread = new Thread(() => SB.Customer(1));
                customerThread.Start();

                Assert.True(customerThread.Join(2000),
                    "Поток клиента не завершился за 2 секунды");

                var queue = (Queue<int>)waitingRoomField!.GetValue(null)!;
                Assert.Single(queue);
                Assert.Equal(1, queue.Peek());

                string text = output.ToString();
                Assert.Contains("Клиент 1 вошел. Мест в очереди: 1", text);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}