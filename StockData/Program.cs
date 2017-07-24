using System;
using WebSocketSharp;

namespace StockData
{
    class Program
    {
        public static void Main(string[] args)
        {
            Intrinio i = new Intrinio("690bad9129baaa47cc63b2b5e30013d1", "1e23122db6779bb784abfdd461a184a7");
            i.RefreshToken();

            i.Connect();

            Console.ReadLine();

            i.Disconnect();


            Console.Write(GraphGenerator.Instance.ToString());

            Console.ReadLine();

        

        }

        public static void GetData()
        {
            //Get info From DB

            //Execute Http Request

            //Save Data on Database


        }
    }
}