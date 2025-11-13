using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace ThreadSemaphoreApp.Tests;

public class SemaphoreTests : IDisposable
{
    private const string TestFile = "test_output.txt";
    private readonly string _originalFilePath;

    public SemaphoreTests()
    {
        _originalFilePath = Program.FilePath;
        Program.FilePath = TestFile;
        ClearFile();
    }

    public void Dispose()
    {
        Program.FilePath = _originalFilePath;
        ClearFile();
    }

    private void ClearFile()
    {
        if (File.Exists(TestFile))
            File.Delete(TestFile);
    }

    [Fact]
    public async Task All_Good_10_Times()
    {
        // 10 РАЗ ЗАПУСК Program 
        for (int run = 1; run <= 10; run++)
        {
            ClearFile();
            await Program.Main(Array.Empty<string>());

            string[] lines = File.ReadAllLines(TestFile);
            string text = File.ReadAllText(TestFile);

            // 1. 10 ПОТОКОВ
            lines.Count(l => l.StartsWith("Поток ")).Should().Be(10, $"Запуск {run}: должно быть 10 потоков");

            // 2. 10 ЧИСЕЛ — НА СЛЕДУЮЩЕЙ СТРОКЕ
            for (int i = 1; i <= 10; i++)
            {
                string line = lines.FirstOrDefault(l => l.StartsWith($"Поток {i}:"));
                line.Should().NotBeNull($"Запуск {run}: Поток {i} не найден");

                string nums = line!.Split(':', 2)[1].Trim();
                nums.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Should().HaveCount(10, $"Запуск {run}: Поток {i} — 10 чисел");
            }
 
            text.Split("\n\nПоток ").Length.Should().Be(11, $"Запуск {run}: должен быть отступ \\n\\n");
        }
    }
}