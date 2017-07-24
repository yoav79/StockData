using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StockData
{
    public class GraphGenerator
    {
        private static volatile GraphGenerator _instance;
        private static readonly object SyncRoot = new Object();

        private Dictionary<string, List<StockVela>> db;

        private GraphGenerator()
        {
            db = new Dictionary<string, List<StockVela>>();
        }

        public static GraphGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new GraphGenerator();
                    }
                }

                return _instance;
            }
        }

        public void AddQuote(Quote qu)
        {

            //revisa si existe el simbolo
            if (db.ContainsKey(qu.Ticker))
            {
                var list = db[qu.Ticker];

                var stockVela = list.SingleOrDefault(a => a.Minute == qu.Minute);

                if (stockVela == null)
                {
                    LastVela(qu.Ticker);
                    stockVela = new StockVela(qu);
                    list.Add(stockVela);
                }
                else
                {
                    stockVela.ProcessValue(qu);
                }
            }
            else
            {
                var list = new List<StockVela>();
                db.Add(qu.Ticker, list);
                list.Add(new StockVela(qu));
            }
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            foreach (var key in db.Keys)
            {
                foreach (var stockVela in db[key])
                {
                    sb.AppendLine(stockVela.Symbol);
                    sb.AppendLine(stockVela.Minute.ToString());
                    sb.AppendLine(stockVela.Time);
                    sb.AppendLine("Max Value   :" + stockVela.MaxValue.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Open Value  :" + stockVela.OpenValue.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Close Value :" + stockVela.CloseValue.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("Min Value   :" + stockVela.MinValue.ToString(CultureInfo.InvariantCulture));
                    sb.AppendLine("---------------------------");
                }
            }

            return sb.ToString();
        }

        private void LastVela(string symbol)
        {

            StringBuilder sb = new StringBuilder();

            var stockVela = db[symbol].LastOrDefault();
            if (stockVela != null)
            {

                sb.AppendLine(stockVela.Symbol);
                sb.AppendLine(stockVela.Minute.ToString());
                sb.AppendLine(stockVela.Time);
                sb.AppendLine("Max Value   :" + stockVela.MaxValue.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Open Value  :" + stockVela.OpenValue.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Close Value :" + stockVela.CloseValue.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Min Value   :" + stockVela.MinValue.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("---------------------------");
            }
     
            Console.WriteLine(sb.ToString());
        }
    }
}