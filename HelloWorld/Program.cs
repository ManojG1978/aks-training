using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World, [Timestamp: {DateTime.Now.ToString()}, Host: {System.Net.Dns.GetHostName()}]");
        }
    }
}
