using System;
using System.Collections.Generic;
using System.Security.AccessControl;

class BankSystem
{
    private List<BankAccount> accounts = new List<BankAccount>();
    private int accountCounter = 1001;

    public void CreateAccount()
    {
        Console.Write("Enter your name: ");
        string owner = Console.ReadLine();
        Console.Write("Enter initial balance: ");
        double initialBalance = Convert.ToDouble(Console.ReadLine());

        BankAccount newAccount = new BankAccount(accountCounter, owner, initialBalance);
        accounts.Add(newAccount);
        Console.WriteLine($"Account Created Successfully! Your Account Number is: {accountCounter}");
        accountCounter++;
    }

    public BankAccount GetAccount(int accountNumber)
    {
        return accounts.Find(acc => acc.AccountNumber == accountNumber); 
    }
    public void PerformBankOperations()
    {
        while (true)
        {
            Console.WriteLine("\n--- Bank Menu ---");
            Console.WriteLine("1. Create Account");
            Console.WriteLine("2. Deposit Money");
            Console.WriteLine("3. Withdraw Money");
            Console.WriteLine("4. View Balance");
            Console.WriteLine("5. Exit");
            Console.Write("Choose an option: ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    CreateAccount();
                    break;

                case 2:
                    Console.Write("Enter Account Number: ");
                    int accNumDeposit = Convert.ToInt32(Console.ReadLine());
                    BankAccount accDeposit = GetAccount(accNumDeposit);
                    if (accDeposit != null)
                    {
                        Console.Write("Enter amount to deposit: ");
                        double amount = Convert.ToDouble(Console.ReadLine());
                        accDeposit.Deposit(amount);
                    }
                    else
                    {
                        Console.WriteLine("Account not found.");
                    }
                    break;

                case 3:
                    Console.Write("Enter Account Number: ");
                    int accNumWithdraw = Convert.ToInt32(Console.ReadLine());
                    BankAccount accWithdraw = GetAccount(accNumWithdraw);
                    if (accWithdraw != null)
                    {
                        Console.Write("Enter amount to withdraw: ");
                        double amount = Convert.ToDouble(Console.ReadLine());
                        accWithdraw.Withdraw(amount);
                    }
                    else
                    {
                        Console.WriteLine("Account not found.");
                    }
                    break;

                case 4: 
                    Console.Write("Enter Account Number: ");
                    int accNumView = Convert.ToInt32(Console.ReadLine());
                    BankAccount accView = GetAccount(accNumView);
                    if (accView != null)
                    {
                        accView.ShowBalance();
                    }
                    else
                    {
                        Console.WriteLine("Account not found.");
                    }
                    break;

                case 5:
                    Console.WriteLine("Exiting Bank System...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}
