using System;

[Serializable]
public class BankException : Exception
{
    public string? AccountNumber { get; }

    public BankException(string message, string? accountNumber = null)
        : base(message)
    {
        AccountNumber = accountNumber;
    }

    public BankException(string message, Exception innerException, string? accountNumber = null)
        : base(message, innerException)
    {
        AccountNumber = accountNumber;
    }
}

[Serializable]
public class InsufficientFundsException : BankException
{
    public decimal Balance { get; }
    public decimal RequestedAmount { get; }
    public decimal Deficit => RequestedAmount - Balance;

    public InsufficientFundsException(decimal balance, decimal requested, string accountNumber)
        : base($"Недостатньо коштів. Баланс: {balance:F2}, запит: {requested:F2}, дефіцит: {requested - balance:F2}",
               accountNumber)
    {
        Balance = balance;
        RequestedAmount = requested;
    }
}

[Serializable]
public class AccountLockedException : BankException
{
    public string LockReason { get; }

    public AccountLockedException(string reason, string accountNumber)
        : base($"Рахунок заблоковано: {reason}", accountNumber)
    {
        LockReason = reason;
    }
}

[Serializable]
public class TransactionLimitException : BankException
{
    public decimal Limit { get; }
    public decimal AttemptedAmount { get; }

    public TransactionLimitException(decimal limit, decimal attempted, string accountNumber)
        : base($"Перевищено ліміт транзакції. Ліміт: {limit:F2}, спроба: {attempted:F2}",
               accountNumber)
    {
        Limit = limit;
        AttemptedAmount = attempted;
    }
}

public class BankAccount
{
    public string AccountNumber { get; }
    public string OwnerName { get; }
    public decimal Balance { get; private set; }
    public decimal DailyLimit { get; set; }
    public bool IsLocked { get; private set; }
    public string? LockReason { get; private set; }

    private decimal _dailySpent;

    public BankAccount(string accountNumber, string owner, decimal balance, decimal dailyLimit = 50000m)
    {
        AccountNumber = accountNumber;
        OwnerName = owner;
        Balance = balance;
        DailyLimit = dailyLimit;
    }

    public void Lock(string reason)
    {
        IsLocked = true;
        LockReason = reason;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сума повинна бути додатною");

        // Перевірка блокування має пріоритет над лімітом і балансом
        if (IsLocked)
            throw new AccountLockedException(LockReason ?? "Невідома причина", AccountNumber);

        if (_dailySpent + amount > DailyLimit)
            throw new TransactionLimitException(DailyLimit, amount, AccountNumber);

        if (amount > Balance)
            throw new InsufficientFundsException(Balance, amount, AccountNumber);

        Balance -= amount;
        _dailySpent += amount;
        Console.WriteLine($"  Знято {amount:F2} грн з рахунку {AccountNumber}. Залишок: {Balance:F2} грн");
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сума повинна бути додатною");

        if (IsLocked)
            throw new AccountLockedException(LockReason ?? "Невідома причина", AccountNumber);

        Balance += amount;
        Console.WriteLine($"  Зараховано {amount:F2} грн на рахунок {AccountNumber}. Баланс: {Balance:F2} грн");
    }
}

class Program
{
    static void Main()
    {
        var account1 = new BankAccount("UA001", "Олена Петренко", 10000m, dailyLimit: 20000m);
        var account2 = new BankAccount("UA002", "Андрій Коваленко", 5000m);

        TestTransaction("Тест 1: Звичайне зняття", () => account1.Withdraw(3000m));
        TestTransaction("Тест 2: Поповнення рахунку", () => account2.Deposit(1500m));
        TestTransaction("Тест 3: Недостатньо коштів", () => account2.Withdraw(10000m));
        TestTransaction("Тест 4: Перевищення ліміту", () => account1.Withdraw(25000m));

        account2.Lock("Підозріла активність");
        TestTransaction("Тест 5: Заблокований рахунок", () => account2.Withdraw(100m));

        TestTransaction("Тест 6: Від'ємна сума", () => account1.Withdraw(-500m));
    }

    static void TestTransaction(string testName, Action transaction)
    {
        Console.WriteLine($"\n{testName}:");
        try
        {
            transaction();
        }
        // Спочатку похідні винятки, потім базовий BankException і системні
        catch (AccountLockedException ex)
        {
            Console.WriteLine($"  ЗАБЛОКОВАНО: {ex.Message}");
            Console.WriteLine($"  Рахунок: {ex.AccountNumber}, Причина: {ex.LockReason}");
        }
        catch (TransactionLimitException ex)
        {
            Console.WriteLine($"  ЛІМІТ: {ex.Message}");
            Console.WriteLine($"  Ліміт: {ex.Limit:F2}, Спроба: {ex.AttemptedAmount:F2}");
        }
        catch (InsufficientFundsException ex)
        {
            Console.WriteLine($"  НЕДОСТАТНЬО КОШТІВ: {ex.Message}");
            Console.WriteLine($"  Дефіцит: {ex.Deficit:F2} грн");
        }
        catch (BankException ex)
        {
            Console.WriteLine($"  БАНКІВСЬКА ПОМИЛКА: {ex.Message}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  НЕВІРНИЙ АРГУМЕНТ: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  НЕВІДОМА ПОМИЛКА: {ex.GetType().Name}: {ex.Message}");
        }
    }
}