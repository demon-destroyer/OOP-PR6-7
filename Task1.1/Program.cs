using System;
using System.Collections.Generic;

public class ShoppingCart
{
    private List<(string Name, decimal Price, int Quantity)> _items = new();

    public void AddItem(string name, decimal price, int quantity)
    {
        _items.Add((name, price, quantity));
    }

    public decimal CalculateTotal()
    {
        decimal total = 0;
        foreach (var item in _items)
        {
            total += item.Price * item.Quantity; // Виправлено 1: * замість +
        }
        return total;
    }

    public decimal ApplyDiscount(decimal total, int discountPercent)
    {
        decimal discount = total * discountPercent / 100m; // Виправлено 2: ділення на 100m
        return total - discount;                           // Виправлено 3: - замість +
    }

    public decimal CalculateTax(decimal amount, double taxRate)
    {
        return amount * (decimal)taxRate; // Виправлено 4: повертає лише суму податку
    }

    public void PrintReceipt()
    {
        Console.WriteLine("=== ЧЕК ===");
        foreach (var item in _items)
        {
            decimal itemTotal = item.Price * item.Quantity; // Виправлено 5: * замість +
            Console.WriteLine($"  {item.Name}: {item.Price:F2} x {item.Quantity} = {itemTotal:F2} грн");
        }

        decimal total = CalculateTotal();
        decimal discountedTotal = ApplyDiscount(total, 10);
        decimal tax = CalculateTax(discountedTotal, 0.20);

        Console.WriteLine($"  Сума: {total:F2} грн");
        Console.WriteLine($"  Знижка 10%: -{total - discountedTotal:F2} грн");
        Console.WriteLine($"  Податок 20%: +{tax:F2} грн");
        Console.WriteLine($"  До сплати: {discountedTotal + tax:F2} грн");
    }
}

class Program
{
    static void Main()
    {
        var cart = new ShoppingCart();
        cart.AddItem("Ноутбук", 25000m, 1);
        cart.AddItem("Миша", 500m, 2);
        cart.AddItem("USB-кабель", 150m, 3);

        cart.PrintReceipt();
    }
}