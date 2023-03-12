using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace rollSales
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // prepare consts
            const float price_per_roll = 0.85F;
            const float cost_per_roll = 0.123F; // needs review

            // prepare db connection
            const string cs = "Data Source = c:/users/miron/Git/rollSales/rollSales.db; Version = 3;";
            using var con = new SQLiteConnection(cs);
            con.Open();
            bool exit = false;
            while (!exit)
            {
                using var status_cmd = new SQLiteCommand(con);

                status_cmd.CommandText = "SELECT SQLITE_VERSION();";
                string version = status_cmd.ExecuteScalar().ToString();

                Console.WriteLine($"SQLite version: {version}");

                Console.WriteLine("Welcome to rollSales!");

                Console.WriteLine("\nCurrent State:\n");
                status_cmd.CommandText = "SELECT * from customers;";
                using SQLiteDataReader rdr = status_cmd.ExecuteReader();
                Console.WriteLine($"{rdr.GetName(0),-10} | {rdr.GetName(1),-9} | {rdr.GetName(3),-6} | debt(euro)");
                Console.WriteLine("--------------------------------------------------");

                float unpaid_cost = 0;
                List<string> customer_names = new List<string>();

                while (rdr.Read())
                {
                    unpaid_cost += rdr.GetInt32(4) * cost_per_roll;
                    Console.WriteLine($"{rdr.GetString(0),-10} | {rdr.GetInt32(1),-9} | {rdr.GetInt32(3), 11} | {rdr.GetInt32(3) * price_per_roll,10}");
                    customer_names.Add(rdr.GetString(0));
                }

                Console.WriteLine($"\nUnpaid cost: {unpaid_cost}");

                Console.WriteLine("\nActions:");
                Console.WriteLine("  reset costs (c)");
                Console.WriteLine("  customer payment (p)");
                Console.WriteLine("  manage weekend (w)");

                string action = Console.ReadLine();

                using var update_cmd = new SQLiteCommand(con);

                switch (action)
                {
                    case "c":
                        // reset paid costs
                        Console.WriteLine("Fully reset all costs?(y, n)");
                        string reset = Console.ReadLine();

                        if (reset == "y")
                        {
                            update_cmd.CommandText = $"UPDATE customers SET unpaidcosts = 0";
                            update_cmd.ExecuteNonQuery();
                        }
                        break;
                    case "p":
                        // file customer payment
                        Console.WriteLine("Enter customer name:");
                        string customer_name = Console.ReadLine();
                        Console.WriteLine("Enter payment(num rolls):");
                        string payment = Console.ReadLine();
                        update_cmd.CommandText = $"UPDATE customers SET unpaidrolls = unpaidrolls-{payment} WHERE name = '{customer_name}'";
                        update_cmd.ExecuteNonQuery();
                        update_cmd.CommandText = $"INSERT INTO logs VALUES ('{DateTime.Now.ToShortDateString()}', '{customer_name}', -{payment});";
                        update_cmd.ExecuteNonQuery();
                        break;
                    case "w":
                        // manage weekend
                        Console.WriteLine("Standard Weekend?((y), n, c)");
                        string mode = Console.ReadLine();

                        if (mode == "y"| mode == "")
                        {
                            update_cmd.CommandText = "UPDATE customers SET unpaidrolls = unpaidrolls+stdamount, unpaidcosts = unpaidcosts+stdamount;";
                        
                        } else if (mode == "n")
                        {
                            Console.WriteLine("Enter desired amount after customer name, or leave free for stdamount!");
                            for (int i = 0; i < customer_names.Count; i++)
                            {
                                Console.WriteLine(customer_names[i]);
                                string temp_amount_raw = Console.ReadLine();

                                // update temp amount in db
                                if (temp_amount_raw == "")
                                {
                                    update_cmd.CommandText = $"UPDATE customers SET tempamount = stdamount WHERE name = '{customer_names[i]}';";
                                }
                                else {
                                    int temp_amount = Int32.Parse(temp_amount_raw);
                                    update_cmd.CommandText = $"UPDATE customers SET tempamount = {temp_amount} WHERE name = '{customer_names[i]}';";
                                }
                                update_cmd.ExecuteNonQuery();
                            }
                            update_cmd.CommandText = "UPDATE customers SET unpaidrolls = unpaidrolls+tempamount, unpaidcosts = unpaidcosts+tempamount;";
                        } else
                        {
                            break;
                        }

                        // update customers values
                        update_cmd.ExecuteNonQuery();
                        if (update_cmd.CommandText.Contains("tempamount"))
                        {
                            update_cmd.CommandText = $"INSERT INTO logs SELECT '{DateTime.Now.ToShortDateString()}', name, tempamount FROM customers;";
                        } else {
                            update_cmd.CommandText = $"INSERT INTO logs SELECT '{DateTime.Now.ToShortDateString()}', name, stdamount FROM customers;";
                        }
                        update_cmd.ExecuteNonQuery();

                        break;
                    default:
                        exit = true;
                        return;
                }
            }

        }
    }
}
