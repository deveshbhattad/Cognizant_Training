using System;
using System.Collections.Generic;

namespace ConsoleApp2
{
    class Program
    {
        static List<BankSystem> accounts = new List<BankSystem>(); 
        static int accountCounter = 1001;
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n🏦 Bank Management System 🏦");
                Console.WriteLine("1. Create Account");
                Console.WriteLine("2. Deposit Money");
                Console.WriteLine("3. Withdraw Money");
                Console.WriteLine("4. Check Balance");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");

                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        CreateAccount();
                        break;
                    case 2:
                        PerformTransaction("deposit");
                        break;
                    case 3:
                        PerformTransaction("withdraw");
                        break;
                    case 4:
                        CheckBalance();
                        break;
                    case 5:
                        Console.WriteLine("Thank you for using the Bank Management System!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select a valid option.");
                        break;
                }
            }
        }

        static void CreateAccount()
        {
            Console.Write("Enter Account Holder Name: ");
            string name = Console.ReadLine();
            Console.Write("Enter Initial Deposit Amount: ₹");
            decimal initialBalance;
            if (!decimal.TryParse(Console.ReadLine(), out initialBalance))
            {
                Console.WriteLine("Invalid amount! Account creation failed.");
                return;
            }

            BankSystem newAccount = new BankSystem(accountCounter, name, initialBalance);
            accounts.Add(newAccount);
            Console.WriteLine($"Account Created Successfully! Account Number: {accountCounter}");
            accountCounter++;
        }

        static void PerformTransaction(string type)
        {
            Console.Write("Enter Account Number: ");
            int accountNumber;
            if (!int.TryParse(Console.ReadLine(), out accountNumber))
            {
                Console.WriteLine("Invalid account number!");
                return;
            }

            BankSystem account = accounts.Find(acc => acc.Account_no == accountNumber);
            if (account == null)
            {
                Console.WriteLine("Account not found!");
                return;
            }

            Console.Write($"Enter Amount to {type}: ₹");
            decimal amount;
            if (!decimal.TryParse(Console.ReadLine(), out amount))
            {
                Console.WriteLine("Invalid amount!");
                return;
            }

            if (type == "deposit")
            {
                account.deposit(amount);
            }
            else if (type == "withdraw")
            {
                account.Withdraw(amount);
            }
        }

        static void CheckBalance()
        {
            Console.Write("Enter Account Number: ");
            int accountNumber;
            if (!int.TryParse(Console.ReadLine(), out accountNumber))
            {
                Console.WriteLine("Invalid account number!");
                return;
            }

            BankSystem account = accounts.Find(acc => acc.Account_no == accountNumber);
            if (account == null)
            {
                Console.WriteLine("Account not found!");
                return;
            }

            account.display();
        }
    }
}
