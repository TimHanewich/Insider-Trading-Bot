using System;
using System.Threading.Tasks;
using Yahoo.Finance;
using System.Collections.Generic;
using SimulatedInvesting;

namespace Insider_Trading_Bot
{
    public class InsiderTradingTask
    {
        public DateTime InsiderTradeFiledAtUtc {get; set;}
        public string Symbol {get; set;}

        //Trading parameters
        public float BuyDollarsWorth {get; set;} //How much of the stock should be purchased (in $'s)
        
        //The two metrics that will trigger a sell off - in other words, these are the two outs to the holding. If either of these turn true, the stock will be sold.
        public float AnticipatedVolumePercentJump {get; set;} //The percent jump in volume that is anticipated as a result of the insider trading news. After the volume goes up by this amount, the shares will be sold.
        public float AnticipatedPricePercentJump {get; set;} //The percent jump in price that will cause a sell off.
        public TimeSpan AutoSellTimeOut {get; set;} //If the above two metrics are not hit in a certain amount of time, dump it.


        //Status reporting
        public bool Active {get; set;}

        public InsiderTradingTask()
        {
            Statuses = new List<string>();

            //Default parameters
            BuyDollarsWorth = 3000f;
            AnticipatedVolumePercentJump = 0.02f;
            AnticipatedPricePercentJump = 0.015f;
            AutoSellTimeOut = new TimeSpan(0, 15, 0);
        }

        public InsiderTradingTask(string symbol, DateTime insider_traded_at_utc)
        {
            Symbol = symbol;
            InsiderTradeFiledAtUtc = insider_traded_at_utc;
        }

        public async Task StartAsync(SimulatedPortfolio portfolio)
        {
            Active = true;

            //Get data
            UpdateStatus("Getting data...");
            Equity e = Equity.Create(Symbol);
            await e.DownloadSummaryAsync();

            //Get quantity we can afford
            UpdateStatus("How much can we afford on " + BuyDollarsWorth.ToString("#,##0") + "?");
            int QuantityToBuy = Convert.ToInt32(Math.Floor(Convert.ToDecimal(BuyDollarsWorth / e.Summary.Price)));
            if (QuantityToBuy == 0)
            {
                Active = false;
                throw new Exception("Unable to afford one share of " + Symbol.ToUpper() + "!");
            }
            UpdateStatus(QuantityToBuy.ToString("#,##0") + " shares are affordable.");

            //Buy it
            UpdateStatus("Purchasing...");
            try
            {
                await portfolio.TradeEquityAsync(Symbol, QuantityToBuy, TransactionType.Buy);
            }
            catch (Exception ex)
            {
                Active = false;
                throw new Exception("Fatal failure while purchasing " + Symbol.ToUpper() + ". Msg: " + ex.Message);
            }
            UpdateStatus("Purchase successful.");

            //Wait to sell
            UpdateStatus("Entering waiting period.");
            bool DropTriggered = false;
            while (DropTriggered == false)
            {
                //Check if it is already over time
                UpdateStatus("Checking timeout...");
                TimeSpan et = TimeSinceInsiderTrade();
                if (et.TotalMinutes >= AutoSellTimeOut.TotalMinutes)
                {
                    DropTriggered = true;
                }

                //Get data?
                Equity ne = Equity.Create(Symbol);
                if (DropTriggered == false)
                {
                    await ne.DownloadSummaryAsync();
                }

                //Check volume
                if (DropTriggered == false)
                {
                    UpdateStatus("Checking volume jump...");
                    float percentvoljump = Convert.ToSingle(ne.Summary.Volume - e.Summary.Volume) / Convert.ToSingle(e.Summary.Volume);
                    if (percentvoljump > AnticipatedVolumePercentJump)
                    {
                        UpdateStatus("Volume jump triggered!");
                        DropTriggered = true;
                    }
                    else
                    {
                        UpdateStatus("Volume jump insufficient.");
                    }
                }

                //Check price
                if (DropTriggered == false)
                {
                    UpdateStatus("Checking price jump...");
                    float percentpricejump = Convert.ToSingle(ne.Summary.Price - e.Summary.Price) / Convert.ToSingle(e.Summary.Price);
                    if (percentpricejump > AnticipatedPricePercentJump)
                    {
                        UpdateStatus("Price jump triggered!");
                        DropTriggered = true;
                    }
                    else
                    {
                        UpdateStatus("Price jump insufficient.");
                    }
                }
            }

            //Actually dump them
            UpdateStatus("Sell trigger occured. Proceeding to sell.");
            try
            {
                await portfolio.TradeEquityAsync(Symbol, QuantityToBuy, TransactionType.Sell);
            }
            catch (Exception ex)
            {
                Active = false;
                throw new Exception("Selling failed! Msg: " + ex.Message);
            }
        }

        #region "Status reporting"

        private List<string> Statuses;

        public void UpdateStatus(string status)
        {
            Statuses.Insert(0, status);

            //If there are more than 20, trim
            while (Statuses.Count > 20)
            {
                Statuses.RemoveAt(Statuses.Count - 1);
            }
        }

        public string[] GetStatuses()
        {
            return Statuses.ToArray();
        }

        #endregion

        public async Task<float> GetAverageVolumePerMinuteAsync(string symbol)
        {
            //Get data
            Equity e = Equity.Create(symbol);
            await e.DownloadSummaryAsync();

            //Get elapsed minutes since market open
            DateTime market_open = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 30, 0); //UTC time the market opened (9:30 PM EST = 13:30 UTC)
            TimeSpan elapsed = DateTime.UtcNow - market_open;
            double elapsed_mins = elapsed.TotalMinutes;

            //Calculate and return
            float VolumePerMinute = Convert.ToSingle(e.Summary.Volume) / Convert.ToSingle(elapsed_mins);
            return VolumePerMinute;
        }

        private TimeSpan TimeSinceInsiderTrade()
        {
            TimeSpan ts = DateTime.UtcNow - InsiderTradeFiledAtUtc;
            return ts;
        }

    }
}