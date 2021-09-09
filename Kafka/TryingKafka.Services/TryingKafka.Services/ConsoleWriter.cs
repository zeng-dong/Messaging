using System;

namespace TryingKafka.KafkaService
{
    public static class ConsoleWriter
    {
        public static void Information(string message, params string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }

        public static void PublishValid(string message, params string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }

        public static void SubscribeReceived(string message, params string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }

        public static void ReportInvalid(string message, params string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }
    }
}
