using System;

class Program
{
    static void Main()
    {
        DatabaseHelper dbHelper = new DatabaseHelper();

        Console.WriteLine("Choose an operation:");
        Console.WriteLine("1. Insert Customer");
        Console.WriteLine("2. Retrieve Customers");
        Console.WriteLine("3. Update Account Balance");
        Console.WriteLine("4. Rename Customer");
        Console.Write("Enter your choice (1-4): ");
        int choice = Convert.ToInt32(Console.ReadLine());

        switch (choice)
        {
            case 1:
                Console.WriteLine("\n--- Insert Customer ---");
                Console.Write("Enter Name: ");
                string name = Console.ReadLine();
                Console.Write("Enter Email: ");
                string email = Console.ReadLine();
                Console.Write("Enter Phone: ");
                string phone = Console.ReadLine();
                Console.Write("Enter Address: ");
                string address = Console.ReadLine();

                dbHelper.InsertCustomer(name, email, phone, address);
                break;

            case 2:
                Console.WriteLine("\n--- Retrieve Customers ---");
                dbHelper.GetCustomers();
                break;

            case 3:
                Console.WriteLine("\n--- Update Account Balance ---");
                Console.Write("Enter Account Number: ");
                int accountNumber = Convert.ToInt32(Console.ReadLine());
                Console.Write("Enter Amount to Add: ");
                decimal amount = Convert.ToDecimal(Console.ReadLine());

                dbHelper.UpdateAccountBalance(accountNumber, amount);
                break;

            case 4:
                Console.WriteLine("\n--- Rename Customer ---");
                Console.Write("Enter Customer ID: ");
                int customerId = Convert.ToInt32(Console.ReadLine());
                Console.Write("Enter New Name: ");
                string newName = Console.ReadLine();

                dbHelper.RenameCustomer(customerId, newName);
                break;

            default:
                Console.WriteLine("Invalid choice. Please run the program again.");
                break;
        }
    }
}
