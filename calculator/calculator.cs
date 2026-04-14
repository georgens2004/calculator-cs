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
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Empty input");
                    continue;
                }

                string[] parts = input.Split(' ');
                if (parts.Length != 3)
                {
                    Console.WriteLine("Invalid format. Use: number number operator");
                    continue;
                }

                if (!double.TryParse(parts[0], out double num1) ||
                    !double.TryParse(parts[1], out double num2))
                {
                    Console.WriteLine("Invalid numbers");
                    continue;
                }

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
