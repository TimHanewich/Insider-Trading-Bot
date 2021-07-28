using System;
using SimulatedInvesting;
using Newtonsoft.Json;

namespace Insider_Trading_Bot
{
    class Program
    {
        public static SimulatedPortfolio Portfolio;

        static void Main(string[] args)
        {
            InsiderTradingTask itt = new InsiderTradingTask();
            itt.Symbol = "MSFT";
            itt.InsiderTradeFiledAtUtc = DateTime.UtcNow;
            itt.StatusUpdated += PrintStatus;
            itt.ExecuteEquityTrade += ExecuteTradeAsync;

            Portfolio = SimulatedPortfolio.Create("TimHanewich");
            Portfolio.EditCash(50000, CashTransactionType.Edit);

            itt.StartAsync().Wait();

        }

        public static void PrintStatus(string msg)
        {
            Console.WriteLine(DateTime.UtcNow.ToString() + " - " + msg);
        }

        public static void ExecuteTradeAsync(string symbol, int quantity, TransactionType tt)
        {
            try
            {
                Portfolio.TradeEquityAsync(symbol, quantity, tt).Wait();
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
