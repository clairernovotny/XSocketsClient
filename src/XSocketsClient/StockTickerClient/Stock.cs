using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockTickerClient
{
    public class Stock : IEquatable<Stock>
    {
        public bool Equals(Stock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Symbol, other.Symbol);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Stock)obj);
        }

        public override int GetHashCode()
        {
            return (Symbol != null ? Symbol.GetHashCode() : 0);
        }

        public static bool operator ==(Stock left, Stock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Stock left, Stock right)
        {
            return !Equals(left, right);
        }

        public string Symbol { get; set; }
        public decimal DayOpen { get; set; }
        public decimal DayClose { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal LastChange { get; set; }
        public decimal Change { get; set; }
        public double PercentChange { get; set; }
        public decimal Price { get; set; }
    }
}
