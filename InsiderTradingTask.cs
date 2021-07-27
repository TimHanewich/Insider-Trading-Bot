using System;
using System.Threading.Tasks;
using Yahoo.Finance;
using System.Collections.Generic;

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

        #region "Status reporting"

        private List<string> Statuses;

        public void AddStatus(string status)
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

    }
}