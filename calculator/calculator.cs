using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter 2 numbers and an operation sign (ex. 4 5 +)");
        Console.WriteLine("Supported operations: + - / *");
        while (true)
        {
            try
            {
                string[] parts = Console.ReadLine().Split(' ');
                
                double num1 = Convert.ToDouble(parts[0]);
                double num2 = Convert.ToDouble(parts[1]);
                string op = parts[2];
                
                double result = 0;
                
                if (op == "+") result = num1 + num2;
                else if (op == "-") result = num1 - num2;
                else if (op == "*") result = num1 * num2;
                else if (op == "/")
                {
                    if (num2 == 0)
                    {
                        Console.WriteLine("Division by zero");
                        continue;
                    }
                    result = num1 / num2;
                }
                else
                {
                    Console.WriteLine("Invalid operation");
                    continue;
                }
                
                Console.WriteLine(result);
            }
            catch
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}
