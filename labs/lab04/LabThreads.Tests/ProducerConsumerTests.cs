using System.Collections.Concurrent;
using System.Threading;
using Xunit;
using PC = ProducerConsumer.Program;

namespace LabThreads.Tests
{
    public class ProducerConsumerTests
    {
        [Fact]
        public void BlockingCollection_ProducersAndConsumers_EmptyBufferAtEnd()
        {
            const int bufferSize = 5;
            const int producersCount = 2;
            const int consumersCount = 2;
            const int itemsPerProducer = 4;
            int itemsPerConsumer = (producersCount * itemsPerProducer) / consumersCount;

            var buffer = new BlockingCollection<int>(bufferSize);

            var producerThreads = new Thread[producersCount];
            for (int i = 0; i < producersCount; i++)
            {
                int producerId = i;
                producerThreads[i] = new Thread(
                    () => PC.ProducerWithBlockingCollection(producerId, buffer, itemsPerProducer));
                producerThreads[i].Start();
            }

            var consumerThreads = new Thread[consumersCount];
            for (int i = 0; i < consumersCount; i++)
            {
                int consumerId = i;
                consumerThreads[i] = new Thread(
                    () => PC.ConsumerWithBlockingCollection(consumerId, buffer, itemsPerConsumer));
                consumerThreads[i].Start();
            }

            foreach (var t in producerThreads)
            {
                Assert.True(t.Join(10000), "Один из производителей не завершился за 10 секунд");
            }

            buffer.CompleteAdding();

            foreach (var t in consumerThreads)
            {
                Assert.True(t.Join(10000), "Один из потребителей не завершился за 10 секунд");
            }

            Assert.True(buffer.IsCompleted);
            Assert.Empty(buffer);
        }

        [Fact]
        public void Semaphore_ProducersAndConsumers_ZeroCountAtEnd()
        {
            const int bufferSize = 5;
            const int producersCount = 2;
            const int consumersCount = 2;
            const int itemsPerProducer = 4;
            int itemsPerConsumer = (producersCount * itemsPerProducer) / consumersCount;

            var buffer = new int[bufferSize];
            int count = 0, inIndex = 0, outIndex = 0;
            object bufferLock = new object();
            var empty = new SemaphoreSlim(bufferSize, bufferSize);
            var full = new SemaphoreSlim(0, bufferSize);
            bool productionComplete = false;

            var producerThreads = new Thread[producersCount];
            for (int i = 0; i < producersCount; i++)
            {
                int producerId = i;
                producerThreads[i] = new Thread(() =>
                    PC.ProducerWithSemaphore(
                        producerId, buffer, bufferLock, empty, full,
                        ref count, ref inIndex, bufferSize, itemsPerProducer));
                producerThreads[i].Start();
            }

            var consumerThreads = new Thread[consumersCount];
            for (int i = 0; i < consumersCount; i++)
            {
                int consumerId = i;
                consumerThreads[i] = new Thread(() =>
                    PC.ConsumerWithSemaphore(
                        consumerId, buffer, bufferLock, empty, full,
                        ref count, ref outIndex, bufferSize,
                        ref productionComplete, itemsPerConsumer));
                consumerThreads[i].Start();
            }

            foreach (var t in producerThreads)
            {
                Assert.True(t.Join(10000), "Один из производителей не завершился за 10 секунд");
            }

            // Сообщаем потребителям, что производство завершено
            lock (bufferLock)
            {
                productionComplete = true;
            }

            // Будим всех потребителей, чтобы они проверили productionComplete
            for (int i = 0; i < consumersCount; i++)
            {
                full.Release();
            }

            foreach (var t in consumerThreads)
            {
                Assert.True(t.Join(10000), "Один из потребителей не завершился за 10 секунд");
            }

            // В буфере не должно остаться элементов
            Assert.Equal(0, count);
        }
    }
}