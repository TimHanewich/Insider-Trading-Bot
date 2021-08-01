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
            InsiderTradingTask itt = new InsiderTradingTask();
            itt.Symbol = "MSFT";
            itt.InsiderTradeFiledAtUtc = DateTime.UtcNow;
            itt.StatusUpdated += PrintStatus;
            itt.ExecuteEquityTrade += ExecuteTradeAsync;
            Task t = itt.StartAsync();

            while (true)
            {
                Console.WriteLine("here");
                Task.Delay(1000).Wait();
            }
        }

        public static void PrintStatus(string msg)
        {
            Console.WriteLine(DateTime.UtcNow.ToString() + " - " + msg);
        }

        public static void ExecuteTradeAsync(string symbol, int quantity, TransactionType tt)
        {
            try
            {
                SimulatedPortfolio Portfolio = PortfolioCloudInterface.DownloadPortfolioAsync().Result;
                Portfolio.TradeEquityAsync(symbol, quantity, tt).Wait();
                PortfolioCloudInterface.UploadPortfolioAsync(Portfolio).Wait();
                Console.WriteLine("Executed trade: " + quantity.ToString("#,##0") + " shares of " + symbol.ToUpper() + " - " + tt.ToString());
            }
            catch (Exception ex)
            {
                AdminWriteLine("Failure while executing trade: " + ex.Message, ConsoleColor.Red);
            }
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
