using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static Queue<int> storage = new Queue<int>();
    static int maxSize;
    static int totalProduction;
    static object lockObject = new object();
    static int producedCount = 0;
    static int consumedCount = 0;

    static void Main(string[] args)
    {
        Console.WriteLine("Введіть максимальну місткість сховища:");
        if (!int.TryParse(Console.ReadLine(), out maxSize) || maxSize <= 0)
        {
            Console.WriteLine("Некоректне значення для максимальної місткості сховища.");
            return;
        }

        Console.WriteLine("Введіть загальну кількість продукції:");
        if (!int.TryParse(Console.ReadLine(), out totalProduction) || totalProduction <= 0)
        {
            Console.WriteLine("Некоректне значення для загальної кількості продукції.");
            return;
        }

        int numProducers = 3; // Кількість виробників
        int numConsumers = 2; // Кількість споживачів

        Thread[] producerThreads = new Thread[numProducers];
        Thread[] consumerThreads = new Thread[numConsumers];

        for (int i = 0; i < numProducers; i++)
        {
            producerThreads[i] = new Thread(Producer);
            producerThreads[i].Start();
        }

        for (int i = 0; i < numConsumers; i++)
        {
            consumerThreads[i] = new Thread(Consumer);
            consumerThreads[i].Start();
        }

        for (int i = 0; i < numProducers; i++)
        {
            producerThreads[i].Join();
        }

        for (int i = 0; i < numConsumers; i++)
        {
            consumerThreads[i].Join();
        }

        Console.WriteLine("Головний потік завершив роботу.");
        Console.ReadLine();
    }

    static void Producer()
    {
        while (true)
        {
            lock (lockObject)
            {
                if (producedCount >= totalProduction)
                {
                    Monitor.PulseAll(lockObject);
                    break;
                }

                while (storage.Count >= maxSize || producedCount >= totalProduction)
                {
                    Monitor.Wait(lockObject);
                }

                producedCount++;
                storage.Enqueue(producedCount);
                Console.WriteLine("Виробник додав число: {1}", Thread.CurrentThread.ManagedThreadId, producedCount);

                Monitor.PulseAll(lockObject);
            }

            Thread.Sleep(1000);
        }
    }

    static void Consumer()
    {
        while (true)
        {
            lock (lockObject)
            {
                if (consumedCount >= totalProduction && storage.Count == 0)
                {
                    Monitor.PulseAll(lockObject);
                    break;
                }

                while (storage.Count == 0 || consumedCount >= totalProduction)
                {
                    Monitor.Wait(lockObject);
                }

                int number = storage.Dequeue();
                consumedCount++;
                Console.WriteLine("Споживач використав число: {1}", Thread.CurrentThread.ManagedThreadId, number);

                Monitor.PulseAll(lockObject);
            }

            Thread.Sleep(1500);
        }
    }
}
