using System;

public class HttpException : Exception
{
    public int StatusCode { get; }
    public string Url { get; }

    public HttpException(int statusCode, string url, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Url = url;
    }
}

public static class Guard
{
    public static void NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }

    public static void NotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Значення не може бути порожнім", paramName);
    }

    public static void InRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, $"Значення {value} за межами [{min}, {max}]");
    }
}

public class ApiClient
{
    private static readonly Random _random = new();

    public static string SendRequest(string url, string method = "GET")
    {
        Guard.NotEmpty(url, nameof(url));
        Guard.NotEmpty(method, nameof(method));

        int roll = _random.Next(1, 7);
        return roll switch
        {
            1 => throw new HttpException(404, url, $"Ресурс не знайдено: {url}"),
            2 => throw new HttpException(401, url, "Потрібна авторизація"),
            3 => throw new HttpException(500, url, "Внутрішня помилка сервера"),
            4 => throw new HttpException(503, url, "Сервіс тимчасово недоступний"),
            5 => throw new TimeoutException($"Таймаут запиту до {url}"),
            _ => $"{{\"status\": \"ok\", \"url\": \"{url}\"}}"
        };
    }
}

class Program
{
    static void Main()
    {
        string[] urls =
        {
            "https://api.example.com/users",
            "https://api.example.com/orders",
            "https://api.example.com/products"
        };

        foreach (string url in urls)
        {
            Console.WriteLine($"\nЗапит: GET {url}");

            try
            {
                string response = ApiClient.SendRequest(url);
                Console.WriteLine($"  Відповідь: {response}");
            }
            // Стоїть першим: логує будь-який виняток і повертає false, передаючи його далі
            catch (Exception ex) when (LogException(ex))
            {
            }
            catch (HttpException ex) when (ex.StatusCode == 404)
            {
                Console.WriteLine($"  [404] Не знайдено: {ex.Url}");
            }
            catch (HttpException ex) when (ex.StatusCode == 401)
            {
                Console.WriteLine($"  [401] Потрібна авторизація для {ex.Url}");
            }
            catch (HttpException ex) when (ex.StatusCode >= 500 && ex.StatusCode < 600)
            {
                Console.WriteLine($"  [{ex.StatusCode}] Серверна помилка: {ex.Message}");
                Console.WriteLine("  Повторна спроба через 5 секунд...");
            }
            catch (HttpException ex)
            {
                Console.WriteLine($"  [HTTP {ex.StatusCode}] {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"  [TIMEOUT] {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  [АРГУМЕНТ] {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Guard Clause демонстрація ===");
        TestGuard(() => ApiClient.SendRequest(""));
        TestGuard(() => Guard.NotNull<string>(null, "config"));
        TestGuard(() => Guard.InRange(70000, 1, 65535, "port"));
    }

    static bool LogException(Exception ex)
    {
        Console.WriteLine($"  [LOG] {DateTime.Now:HH:mm:ss} — {ex.GetType().Name}: {ex.Message}");
        return false;
    }

    static void TestGuard(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Guard спрацював ({ex.GetType().Name}): {ex.Message}");
        }
    }
}