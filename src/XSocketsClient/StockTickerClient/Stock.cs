using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace StockTickerClient
{
    public class Stock : IEquatable<Stock>, INotifyPropertyChanged
    {
        private string _symbol;
        private decimal _dayOpen;
        private decimal _dayClose;
        private decimal _dayLow;
        private decimal _dayHigh;
        private decimal _lastChange;
        private decimal _change;
        private double _percentChange;
        private decimal _price;

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                OnPropertyChanged();
            }
        }

        public decimal DayOpen
        {
            get { return _dayOpen; }
            set
            {
                _dayOpen = value;
                OnPropertyChanged();
            }
        }

        public decimal DayClose
        {
            get { return _dayClose; }
            set
            {
                _dayClose = value;
                OnPropertyChanged();
            }
        }

        public decimal DayLow
        {
            get { return _dayLow; }
            set
            {
                _dayLow = value;
                OnPropertyChanged();
            }
        }

        public decimal DayHigh
        {
            get { return _dayHigh; }
            set
            {
                _dayHigh = value;
                OnPropertyChanged();
            }
        }

        public decimal LastChange
        {
            get { return _lastChange; }
            set
            {
                _lastChange = value;
                OnPropertyChanged();
            }
        }

        public decimal Change
        {
            get { return _change; }
            set
            {
                _change = value;
                OnPropertyChanged();
            }
        }

        public double PercentChange
        {
            get { return _percentChange; }
            set
            {
                _percentChange = value;
                OnPropertyChanged();
            }
        }

        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public bool Equals(Stock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Symbol, other.Symbol);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}