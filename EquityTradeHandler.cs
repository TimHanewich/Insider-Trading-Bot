using System;
using SimulatedInvesting;

namespace Insider_Trading_Bot
{
    public delegate void EquityTradeHandler(string Symbol, int Quantity, TransactionType tt);
}