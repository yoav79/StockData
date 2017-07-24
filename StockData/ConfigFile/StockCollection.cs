using System.Configuration;

namespace StockData.ConfigFile
{
    public class StockCollection : ConfigurationElementCollection
    {
        public StockElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as StockElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new StockElement this[string responseString]
        {
            get { return (StockElement)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new StockElement();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((StockElement)element).Name;
        }
    }
}