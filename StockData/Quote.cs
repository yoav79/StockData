using System;

namespace StockData
{
    public class Quote
    {
        public string Type;
        
        public string Ticker;
        public int Size;
        public decimal Price;

        public double TimeStamp { get; }

        public DateTime DateStamp { get; }

        public int Minute { get; }

        public Quote(double ts)
        {
            TimeStamp = ts;

            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(TimeStamp).ToLocalTime();
            DateStamp = dtDateTime;

            Minute =
                (int)
                    DateStamp.Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 30, 0))
                        .TotalMinutes;
        }
    }
}