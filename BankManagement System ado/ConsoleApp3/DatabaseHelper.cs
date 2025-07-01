using System;
using System.Data.SqlClient;

public class DatabaseHelper
{
    private string connectionString = "Server=LTIN522006\\SQLEXPRESS;Database=BankManagementSystem;Integrated Security=True;";

    // INSERT Operation
    public void InsertCustomer(string name, string email, string phone, string address)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Customers (Name, Email, Phone, Address) VALUES (@Name, @Email, @Phone, @Address)";
                SqlCommand cmd = new SqlCommand(query, conn); 
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Customer inserted successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }

    // RETRIEVE Operation
    public void GetCustomers()
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Customers";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine($"Customer ID: {reader["CustomerID"]}, Name: {reader["Name"]}");
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }

    // UPDATE Operation
    public void UpdateAccountBalance(int accountNumber, decimal amount)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountNumber = @AccountNumber";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Account balance updated successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }

    // DELETE Operation
    public void DeleteCustomer(int customerId)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // First, delete accounts linked to the customer
                string deleteAccountsQuery = "DELETE FROM Accounts WHERE CustomerID = @CustomerID";
                SqlCommand deleteAccountsCmd = new SqlCommand(deleteAccountsQuery, conn);
                deleteAccountsCmd.Parameters.AddWithValue("@CustomerID", customerId);
                deleteAccountsCmd.ExecuteNonQuery();

                // Now delete the customer
                string query = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Customer and their accounts deleted successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }
    public void RenameCustomer(int customerId, string newName)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Customers SET Name = @NewName WHERE CustomerID = @CustomerID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.Parameters.AddWithValue("@NewName", newName);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Customer name updated successfully.");
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }


}
