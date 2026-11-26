using System;
using System.IO;

public class SafeCalculator
{
    public static double Calculate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Вираз не може бути порожнім.");

        string[] parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
            throw new ArgumentException(
                $"Очікується формат 'число1 оператор число2', отримано: '{expression}'");

        // decimal гарантує OverflowException при переповненні (на відміну від double, що дає Infinity)
        decimal a = decimal.Parse(parts[0]);
        decimal b = decimal.Parse(parts[2]);
        string op = parts[1];

        decimal result = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b != 0 ? a / b : throw new DivideByZeroException("Дільник дорівнює нулю"),
            "%" => b != 0 ? a % b : throw new DivideByZeroException("Дільник дорівнює нулю"),
            _ => throw new ArgumentException($"Невідомий оператор: '{op}'")
        };

        return (double)result;
    }

    public static void LogError(string expression, Exception ex, string logFile = "errors.log")
    {
        StreamWriter? writer = null;
        try
        {
            writer = new StreamWriter(logFile, append: true);
            writer.WriteLine($"[{DateTime.Now}] Вираз: '{expression}' — {ex.GetType().Name}: {ex.Message}");
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"  Помилка запису в лог: {ioEx.Message}");
        }
        finally
        {
            // Безпечне закриття ресурсу навіть якщо сталася помилка при створенні StreamWriter
            writer?.Dispose();
        }
    }
}

class Program
{
    static void Main()
    {
        string[] testExpressions =
        {
            "10 + 5",
            "100 / 0",
            "abc * 2",
            "10 @ 5",
            "10 +",
            "999999999999999999999999999999 + 1",
            ""
        };

        foreach (string expr in testExpressions)
        {
            try
            {
                double result = SafeCalculator.Calculate(expr);
                Console.WriteLine($"  {expr} = {result}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"  ПОМИЛКА формату: {ex.Message}");
                SafeCalculator.LogError(expr, ex);
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"  ПОМИЛКА ділення: {ex.Message}");
                SafeCalculator.LogError(expr, ex);
            }
            catch (OverflowException ex)
            {
                Console.WriteLine($"  ПОМИЛКА переповнення: {ex.Message}");
                SafeCalculator.LogError(expr, ex);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  ПОМИЛКА аргументу: {ex.Message}");
                SafeCalculator.LogError(expr, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  НЕВІДОМА ПОМИЛКА: {ex.GetType().Name}: {ex.Message}");
                SafeCalculator.LogError(expr, ex);
            }
        }
    }
}