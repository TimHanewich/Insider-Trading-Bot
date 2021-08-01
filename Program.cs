using System;
using SimulatedInvesting;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Insider_Trading_Bot
{
    class Program
    {

        static void Main(string[] args)
        {
            
        }

        public static void PrintStatus(string msg)
        {
            Console.WriteLine(DateTime.UtcNow.ToString() + " - " + msg);
        }
  
        public static void AdminWrite(string msg, ConsoleColor cc = ConsoleColor.Cyan)
        {
            ConsoleColor oc = Console.ForegroundColor;
            Console.ForegroundColor = cc;
            Console.Write(msg);
            Console.ForegroundColor = oc;
        }

        public static void AdminWriteLine(string msg, ConsoleColor cc = ConsoleColor.Cyan)
        {
            AdminWrite(msg + Environment.NewLine, cc);
        }


    }
}
