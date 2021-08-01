using System;
using SimulatedInvesting;

namespace Insider_Trading_Bot
{
    public class InsiderTradingBot
    {
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
                Program.AdminWriteLine("Failure while executing trade: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}