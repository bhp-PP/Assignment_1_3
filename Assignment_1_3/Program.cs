using System;
using System.Threading;

namespace Assignment_1_3
{
    class Program
    {
        const int BUFFER_SIZE = 10;
        const int EXIT = -1;

        const int MIN_NUMBER = 1;
        const int MAX_NUMBER = 20;

        const int PRODUCER_DELAY = 500;
        const int CONSUMER_DELAY = 1000;

        private static readonly SemaphoreSlim bufferFull = new SemaphoreSlim(BUFFER_SIZE, BUFFER_SIZE);
        private static readonly SemaphoreSlim bufferEmpty = new SemaphoreSlim(0, BUFFER_SIZE);
        private static readonly SemaphoreSlim bufferGate = new SemaphoreSlim(1);

        static void Main()
        {
            CircularBuffer<int> buffer = new CircularBuffer<int>(BUFFER_SIZE, EXIT);

            Thread producer1 = new Thread(() => Produce(buffer, MIN_NUMBER, MAX_NUMBER, PRODUCER_DELAY));
 
            Thread consumer1 = new Thread(() => Consume(buffer, CONSUMER_DELAY));

            // start the producer-consumer
            producer1.Start();
            consumer1.Start();

            // wait for the producer to finish up
            producer1.Join();

            // wait for the consumer to read the rest of the values from the buffer.
            consumer1.Join();

            Console.WriteLine("All Done");
        }

        public static void Produce(CircularBuffer<int> buffer, int minimumValue, int maximumValue, int delayInMilliSeconds)
        {
            for (int item = minimumValue; item <= maximumValue; item++)
            {
                bufferFull.Wait();

                bufferGate.Wait();
                buffer.Add(item);
                bufferGate.Release();
                
                bufferEmpty.Release();

                Thread.Sleep(delayInMilliSeconds);
            }

            // Write EXIT value to the buffer to stop the consumer
            bufferFull.Wait();
            buffer.Add(buffer.EXIT);
            bufferEmpty.Release();

            Console.WriteLine("Producer Done");
        }

        public static void Consume(CircularBuffer<int> buffer, int delayInMilliSeconds)
        {
            bufferEmpty.Wait();
            int number = buffer.Fetch();
            bufferFull.Release();

            while (number != buffer.EXIT)
            {
                Console.WriteLine(number);
                Thread.Sleep(delayInMilliSeconds);

                bufferEmpty.Wait();
   
                bufferGate.Wait();
                number = buffer.Fetch();
                bufferGate.Release();
                
                bufferFull.Release();
            }

            Console.WriteLine("Consumer Done");
        }
    }
}

