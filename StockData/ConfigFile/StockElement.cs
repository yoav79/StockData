using System.Configuration;

namespace StockData.ConfigFile
{
    public class StockElement : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => this["name"] as string;

        [ConfigurationProperty("symbol", IsRequired = true)]
        public string Symbol => this["symbol"] as string;

        [ConfigurationProperty("spread", IsRequired = false)]
        public int? Spread => this["spread"] as int?;
    }
    
}