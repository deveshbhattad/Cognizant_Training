using System;

class BankAccount
{
    public int AccountNumber { get; private set; }
    public string Owner { get; private set; }
    public double Balance { get; private set; }

    public BankAccount(int accountNumber, string owner, double initialBalance)
    {
        AccountNumber = accountNumber;
        Owner = owner;
        Balance = initialBalance;
    }

    public void Deposit(double amount)
    {
        Balance += amount;
        Console.WriteLine($"Deposited: {amount}. New Balance: {Balance}");
    }

    public void Withdraw(double amount)
    {
        if (amount <= Balance)
        {
            Balance -= amount;
            Console.WriteLine($"Withdrawn: {amount}. Remaining Balance: {Balance}");
        }
        else
        {
            Console.WriteLine("Insufficient funds.");
        }
    }

    public void ShowBalance()
    {
        Console.WriteLine($"Account No: {AccountNumber}, Owner: {Owner}, Balance: {Balance}");
    }
}
