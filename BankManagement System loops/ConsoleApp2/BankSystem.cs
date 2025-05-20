using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class BankSystem
    {
        public string Account_holder_name { get; private set; }
        public int Account_no { get; private set; }
        public decimal Balance { get; private set; }

        public BankSystem(int account_no,
            string account_holder_name,
            decimal intial_balance)
        {
            Account_holder_name = account_holder_name;
            Account_no = account_no;
            Balance = intial_balance;
        }
        public void deposit(decimal amount)
        {
            Balance += amount;
            Console.WriteLine($"Deposited amount {amount} and balance {Balance}");
        }
        public bool Withdraw(decimal amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                Console.WriteLine($"new balance is {Balance} and amount withdrawn is {amount}");
                return true;
            }
            else
            {
                Console.WriteLine("Insufficient Funds");
                return false;
            }
        }
        public void display()
        {
            Console.WriteLine($"Account holder name {Account_holder_name}, having account no  {Account_no} have balance{Balance}");
        }
    };
}
