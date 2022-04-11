using System;
using System.Collections.Generic;
using System.Net;

namespace TheUniversalCity.RedisClient
{
    public class RedisConfiguration
    {
        public const string PASSWORD_KEY = "password";
        public const string DB_KEY = "db";
        public const string CLIENT_CACHE_KEY = "clientCache";
        public const string CONNECT_RETRY_KEY = "connectRetry";
        public const string CONNECT_RETRY_INTERVAL_KEY = "connectRetryInterval";
        public const string RECEIVE_BUFFER_SIZE = "receiveBufferSize";
        public const string SEND_BUFFER_SIZE = "sendBufferSize";

        public List<DnsEndPoint> DnsEndPoints { get; } = new List<DnsEndPoint>();

        public string Password { get { return Options.ContainsKey(PASSWORD_KEY) ? Options[PASSWORD_KEY] : null; } }
        public byte DB { get { return Options.ContainsKey(DB_KEY) ? byte.Parse(Options[DB_KEY]) : (byte)0; } }
        public bool ClientCache { get { return Options.ContainsKey(CLIENT_CACHE_KEY) ? bool.Parse(Options[CLIENT_CACHE_KEY]) : false; } }
        public int ConnectRetry { get { return Options.ContainsKey(CONNECT_RETRY_KEY) ? int.Parse(Options[CONNECT_RETRY_KEY]) : 3; } }
        public int ConnectRetryInterval { get { return Options.ContainsKey(CONNECT_RETRY_INTERVAL_KEY) ? int.Parse(Options[CONNECT_RETRY_INTERVAL_KEY]) : 300; } }
        public int ReceiveBufferSize { get { return Options.ContainsKey(RECEIVE_BUFFER_SIZE) ? int.Parse(Options[RECEIVE_BUFFER_SIZE]) : 65536; } }
        public int SendBufferSize { get { return Options.ContainsKey(SEND_BUFFER_SIZE) ? int.Parse(Options[SEND_BUFFER_SIZE]) : 65536; } }

        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

        public RedisConfiguration(string connectionString)
        {
            var segments = connectionString.Split(',');

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];

                var indexOfEquivelantOperator = segment.IndexOf('=');
                var lastIndexOfEquivelantOperator = segment.LastIndexOf('=');

                if (indexOfEquivelantOperator != lastIndexOfEquivelantOperator)
                {
                    throw new InvalidOperationException($"Invalid entry \"{segment}\"");
                }

                if (indexOfEquivelantOperator == -1)
                {
                    var indexOfColon = segment.IndexOf(':');
                    var lastIndexOfColon = segment.IndexOf(':');

                    if (indexOfColon != lastIndexOfColon)
                    {
                        throw new InvalidOperationException($"Invalid entry \"{segment}\"");
                    }

                    if (indexOfColon != -1)
                    {
                        DnsEndPoints.Add(new DnsEndPoint(segment.Substring(0, indexOfColon), int.Parse(segment.Substring(indexOfColon + 1))));
                    }
                    else
                    {
                        DnsEndPoints.Add(new DnsEndPoint(segment.Substring(0), 6379));
                    }
                }
                else
                {
                    var key = segment.Substring(0, indexOfEquivelantOperator);
                    var value = segment.Substring(indexOfEquivelantOperator + 1);

                    Options.Add(key, value);
                }
            }
        }

        public RedisConfiguration() { }
    }
}
