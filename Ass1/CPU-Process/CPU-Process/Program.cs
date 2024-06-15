using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string numberString = args[0];
        int number;

        if (int.TryParse(numberString, out number))
        {
            calculation(number);
               
        }
        else
        {
            Console.WriteLine("Failed to parse the string as an integer.");
        }

    }
    static void calculation(int number)
    {
        double test = 0;
        for (int i = 0; i < number; i++)
        {
            test += Math.Pow(i, 2) * Math.Tan(i) + Math.Sin(i)*Math.Cos(i);
        }
    }
}