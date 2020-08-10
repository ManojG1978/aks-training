using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World! from from {System.Net.Dns.GetHostName()}");
        }
    }
}
