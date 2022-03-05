using System;
using System.Runtime.Serialization;

namespace TheUniversalCity.RedisClient.Exceptions
{
    public class RedisClientNotConectedException : ApplicationException
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public RedisClientNotConectedException()
        {
        }

        public RedisClientNotConectedException(string message) : base(message)
        {
        }

        public RedisClientNotConectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RedisClientNotConectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Host = info.GetValue(nameof(Host), typeof(string)) as string;
            Port = (int)info.GetValue(nameof(Port), typeof(int));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Host), Host);
            info.AddValue(nameof(Port), Port);
        }
    }
}
