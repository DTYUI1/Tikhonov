using System;
using System.Threading;
using Xunit;
using DP = DiningPhilosophers.Program;

namespace LabThreads.Tests
{
    public class DiningPhilosophersTests
    {
        [Fact]
        public void PhilosopherWithDeadlock_SinglePhilosopher_StopsOnInterrupt()
        {
            var forks = new object[1];
            forks[0] = new object();

            var thread = new Thread(() => DP.PhilosopherWithDeadlock(0, forks));
            thread.Start();

            // Даём потоку стартовать
            Thread.Sleep(500);

            thread.Interrupt();

            Assert.True(
                thread.Join(2000),
                "Философ с deadlock не завершился после Interrupt за 2 секунды");
        }

        [Fact]
        public void PhilosopherWithoutDeadlock_AllPhilosophersStopOnInterrupt()
        {
            const int philosophersCount = 5;
            var forks = new object[philosophersCount];
            for (int i = 0; i < philosophersCount; i++)
                forks[i] = new object();

            // Семафор ограничивает кол-во философов за столом
            var table = new SemaphoreSlim(philosophersCount - 1, philosophersCount - 1);

            var threads = new Thread[philosophersCount];
            for (int i = 0; i < philosophersCount; i++)
            {
                int id = i;
                threads[i] = new Thread(() => DP.PhilosopherWithoutDeadlock(id, forks, table));
                threads[i].Start();
            }

            // Даём системе поработать
            Thread.Sleep(2000);

            // Прерываем всех философов
            foreach (var t in threads)
                t.Interrupt();

            // Если бы был дедлок по вилкам, потоки бы не завершились
            for (int i = 0; i < philosophersCount; i++)
            {
                Assert.True(
                    threads[i].Join(3000),
                    $"Философ {i} в версии без deadlock не завершился за 3 секунды");
            }
        }
    }
}