using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSemaphoreApp;

class Program
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(3, 3);
    private static readonly Lock Lock = new();
    public static string FilePath = "output.txt";

    public static async Task Main(string[] args)
    {
        await File.WriteAllTextAsync(FilePath, string.Empty);
        Console.WriteLine("Запуск 10 потоков (максимум 3 одновременно)...\n");

        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            int id = i + 1;
            tasks[i] = Task.Run(() => Worker(id));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("\nВсе потоки завершили работу.");
        Console.WriteLine($"Результат записан в файл: {Path.GetFullPath(FilePath)}");
    }

    private static async Task Worker(int id)
    {
        Console.WriteLine($"Поток {id} ждёт разрешения семафора");
        await SemaphoreSlim.WaitAsync();
        try
        {
            Console.WriteLine($"Поток {id} записал данные и выходит Генерируются 10 чисел... ");

            var rnd = new Random();
            var numbers = Enumerable.Range(0, 10).Select(_ => rnd.Next(1, 101)).ToArray();

            await Task.Delay(50); 
            
            string content = $"\n\nПоток {id}: {string.Join(" ", numbers)}\n";

            lock (Lock)
            {
                File.AppendAllText(FilePath, content);
            }

            Console.WriteLine($"Поток {id} записал 10 чисел и завершился.");
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }
}