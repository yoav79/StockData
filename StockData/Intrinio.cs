using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json;
using StockData.ConfigFile;
using WebSocketSharp;

namespace StockData
{
    public class Intrinio
    {

        //const
        const int TOKEN_EXPIRATION_INTERVAL = 1000*60*60*24*7; // 1 week
        const int HEARTBEAT_INTERVAL = 1000*20; // 20 seconds
        const int WS_CLOSE_REASON_USER = 1000;
        const string HOST = "realtime.intrinio.com";
        const int PORT = 443;
        const string WS_PROTOCOL = "wss";
        static readonly int[] SELF_HEAL_BACKOFFS = { 0, 100, 500, 1000, 2000, 5000 };

        //private fields
        private readonly string _username;
        private readonly string _password;
        private string _token;

        private List<string> _symbols;

        private Thread _heartbeat;
        private bool _heartbeatAlive;

        private WebSocket _ws;

        public Intrinio(string username, string password)
        {
            if (username == null)
                throw new ArgumentException("username not be null!");

            if (password == null)
                throw new ArgumentException("username not be null!");

            _password = password;
            _username = username;

            _symbols = new List<string>();

            //Get Symbols from config
            var config = RegisterStocksConfig.GetConfig();
            foreach (dynamic item in config.Stocks)
            {
                Console.WriteLine(item.Symbol);
                _symbols.Add(item.Symbol);
            }
        }

        public bool Connect()
        {
            var socketUrl = WS_PROTOCOL + "://" + HOST + ":" + PORT + "/socket/websocket?vsn=1.0.0&token=" + _token;

            _ws = new WebSocket(socketUrl)
            {
                SslConfiguration =
                {
                    ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    EnabledSslProtocols = SslProtocols.Tls12
                }
            };

            //events
            _ws.OnOpen += Ws_OnOpen;
            _ws.OnClose += Ws_OnClose;
            _ws.OnMessage += Ws_OnMessage;
            _ws.OnError += Ws_OnError;

            _ws.Connect();


            foreach (var symbol in _symbols)
            {
                var s = CreateChannel(symbol);
                _ws.Send(s);
            }

            return _ws.IsAlive;
        }
        
        public void Disconnect()
        {

            foreach (var symbol in _symbols)
            {
                var s = RemoveChannel(symbol);
                _ws.Send(s);
            }

            if (_ws.IsAlive)
                _ws.Close();
        }


        // Messages
        public string CreateChannel(string symbol)
        {
            var channel = new
            {
                topic = ParseChannel(symbol),
                @event = "phx_join",
                payload = new {},
                @ref = new { }
            };

            

            return JsonConvert.SerializeObject(channel);

        }

        public string RemoveChannel(string symbol)
        {
            var channel = new
            {
                topic = ParseChannel(symbol),
                @event = "phx_leave",
                payload = new { },
                @ref = new { }
            };



            return JsonConvert.SerializeObject(channel);

        }

        public string ParseChannel(string channel)
        {
            var topic = "";

            if (channel == "$lobby")
            {
                topic = "iex:lobby";
            }
            else if (channel == "$lobby_last_price")
            {
                topic = "iex:lobby:last_price";
            }
            else
            {
                topic = "iex:securities:" + channel;
            }

            return topic;
        }

        public string CreateHeartBeat()
        {
            var channel = new
            {
                topic = "phoenix",
                @event = "heartbeat",
                payload = new { },
                @ref = new { }
            };

            return JsonConvert.SerializeObject(channel);

        }
        
        // Events
        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            //Console.WriteLine(e.Data);

            dynamic q = JsonConvert.DeserializeObject(e.Data);
            string ev = q.@event.ToString();

            switch (ev)
            {
                case "phx_join":

                    break;

                case "phx_leave":
                    break;

                case "quote":
                    var qu = new Quote((double)q.payload.timestamp)
                    {
                        Type = q.payload.type,
                        Ticker = q.payload.ticker,
                        Size = q.payload.size,
                        Price = q.payload.price,
                    };

                    if (qu.Size == 100 && qu.Type == "bid")
                    {
                        var g = GraphGenerator.Instance;
                        g.AddQuote(qu);
                    }

                    break;
                default:
                    break;
            }
        }
        
        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            _heartbeatAlive = false;
            Thread.Sleep(300);
            Console.WriteLine(e.Code);
            Console.WriteLine("Disconnected..");

        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("Connected..");

            _heartbeat = new Thread(() =>
            {
                _heartbeatAlive = true;

                var ws = (WebSocket) sender;

                while (ws.IsAlive)
                {
                    var s = CreateHeartBeat();
                    ws.Send(s);

                    for (var i = 0; i < 200; i++)
                    {
                        Thread.Sleep(100);

                        if (!_heartbeatAlive)
                            return;
                    }

                    if (!_heartbeatAlive)
                        return;
                }
            });

            _heartbeat.Start();
        }

        
        public void RefreshToken()
        {
            //TODO pensar si hay errores ????

            Console.WriteLine("Requesting auth token...");

            var request = WebRequest.Create("https://" + HOST + "/auth");
            request.Method = "GET";

            ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var encoded =
                System.Convert.ToBase64String(
                    System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(_username + ":" + _password));

            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Basic " + encoded);

            var response = request.GetResponse();

            Console.WriteLine(((HttpWebResponse) response).StatusDescription);
            Console.WriteLine(response.ContentLength);

            var dataStream = response.GetResponseStream();

            var reader = new StreamReader(dataStream);

            var responseFromServer = reader.ReadToEnd();

            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();

            _token = responseFromServer;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        
    }
}