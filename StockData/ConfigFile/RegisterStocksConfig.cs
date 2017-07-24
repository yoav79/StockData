using System.Configuration;

namespace StockData.ConfigFile
{
    public class RegisterStocksConfig
 : ConfigurationSection
    {

        public static RegisterStocksConfig GetConfig()
        {
            return (RegisterStocksConfig)System.Configuration.ConfigurationManager.GetSection("RegisterStocks") ?? new RegisterStocksConfig();
        }

        [System.Configuration.ConfigurationProperty("Stocks")]
        [ConfigurationCollection(typeof(StockElement), AddItemName = "Stock")]
        public StockCollection Stocks
        {
            get
            {
                object o = this["Stocks"];
                return o as StockCollection;
            }
        }

    }
}