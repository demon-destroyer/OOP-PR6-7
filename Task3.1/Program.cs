using System;
using System.Collections.Generic;
using System.IO;

public class FileProcessor
{
    public static Dictionary<string, string> ReadConfig(string filePath)
    {
        var config = new Dictionary<string, string>();
        StreamReader? reader = null;

        try
        {
            reader = new StreamReader(filePath);
            string? line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                    throw new FormatException(
                        $"Невірний формат у рядку {lineNumber}: '{line}'. Очікується 'ключ=значення'.");

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (config.ContainsKey(key))
                    throw new InvalidOperationException($"Дублікат ключа '{key}' у рядку {lineNumber}.");

                config[key] = value;
            }

            Console.WriteLine($"Конфігурацію зчитано: {config.Count} параметрів");
            return config;
        }
        // FileNotFoundException та DirectoryNotFoundException наслідують IOException, тому стоять вище
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Файл не знайдено: {ex.FileName}");
            Console.WriteLine("Використовую конфігурацію за замовчуванням...");
            return GetDefaultConfig();
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Каталог не існує. Використовую конфігурацію за замовчуванням...");
            return GetDefaultConfig();
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Немає доступу до файлу: {ex.Message}");
            return GetDefaultConfig();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка введення-виведення: {ex.Message}");
            throw; // Повторне збудження зі збереженням оригінального StackTrace
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Помилка формату: {ex.Message}");
            return GetDefaultConfig();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Помилка структури конфігурації: {ex.Message}");
            return GetDefaultConfig();
        }
        finally
        {
            reader?.Dispose();
            Console.WriteLine("  [finally] Ресурси звільнено.");
        }
    }

    private static Dictionary<string, string> GetDefaultConfig() => new()
    {
        ["host"] = "localhost",
        ["port"] = "8080",
        ["debug"] = "false"
    };

    public static void ValidateConfig(Dictionary<string, string> config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "Конфігурація не може бути null");

        if (!config.ContainsKey("host") || string.IsNullOrWhiteSpace(config["host"]))
            throw new ArgumentException("Відсутній або порожній обов'язковий параметр 'host'");

        if (!config.ContainsKey("port"))
            throw new ArgumentException("Відсутній обов'язковий параметр 'port'");

        if (!int.TryParse(config["port"], out int port))
            throw new FormatException($"Значення порту '{config["port"]}' не є числом");

        if (port < 1 || port > 65535)
            throw new ArgumentOutOfRangeException("port",
                $"Порт {port} за межами допустимого діапазону [1, 65535]");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Тест 1: Неіснуючий файл ===");
        var config1 = FileProcessor.ReadConfig("nonexistent.cfg");
        PrintConfig(config1);

        Console.WriteLine("\n=== Тест 2: Неіснуючий каталог ===");
        var configDir = FileProcessor.ReadConfig("nonexistent_dir/config.cfg");
        PrintConfig(configDir);

        Console.WriteLine("\n=== Тест 3: Правильний файл ===");
        File.WriteAllText("config.cfg", "# Налаштування сервера\nhost=192.168.1.1\nport=3000\ndebug=true");
        var config2 = FileProcessor.ReadConfig("config.cfg");
        PrintConfig(config2);

        Console.WriteLine("\n=== Тест 4: Помилковий формат ===");
        File.WriteAllText("bad_config.cfg", "host=localhost\nINVALID_LINE\nport=8080");
        var config3 = FileProcessor.ReadConfig("bad_config.cfg");
        PrintConfig(config3);

        Console.WriteLine("\n=== Тест 5: Валідація коректної конфігурації ===");
        ValidateAndPrint(config2);

        Console.WriteLine("\n=== Тест 6: Невірний формат порту ===");
        ValidateAndPrint(new Dictionary<string, string> { ["host"] = "localhost", ["port"] = "abc" });

        Console.WriteLine("\n=== Тест 7: Порт поза діапазоном ===");
        ValidateAndPrint(new Dictionary<string, string> { ["host"] = "localhost", ["port"] = "70000" });
    }

    static void ValidateAndPrint(Dictionary<string, string> config)
    {
        try
        {
            FileProcessor.ValidateConfig(config);
            Console.WriteLine("  Конфігурація валідна.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Помилка валідації ({ex.GetType().Name}): {ex.Message}");
        }
    }

    static void PrintConfig(Dictionary<string, string> config)
    {
        foreach (var kvp in config)
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
    }
}