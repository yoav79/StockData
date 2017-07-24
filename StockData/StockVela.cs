using System;

namespace StockData
{
    public class StockVela
    {
        public string Symbol;

        public DateTime Created;
        
        // minute 
        public int Minute { get; }

        public string Time { get; }

        // y
        public decimal MaxValue;

        public decimal MinValue;

        public decimal OpenValue;

        public decimal CloseValue;

        public StockVela(Quote qu)
        {
            Symbol = qu.Ticker;
            Created = DateTime.Now;

            Minute =
                (int)
                    DateTime.Now.Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 30, 0))
                        .TotalMinutes;

            Time = $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00},";

            OpenValue = qu.Price;
            MaxValue = qu.Price;
            MinValue = qu.Price;
            ProcessValue(qu);
        }

        public void ProcessValue(Quote qu)
        {
            CloseValue = qu.Price;

            if (MaxValue < qu.Price)
                MaxValue = qu.Price;

            if (MinValue > qu.Price)
                MinValue = qu.Price;
        }


        public void Save()
        {

        }
    }
}