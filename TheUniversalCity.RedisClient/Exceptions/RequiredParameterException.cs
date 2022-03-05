using System;
using System.Runtime.Serialization;

namespace TheUniversalCity.RedisClient.Exceptions
{
    public class RequiredParameterException : ApplicationException
    {
        public string Key { get; set; }
        public TimeSpan? Expiry { get; set; }

        public RequiredParameterException()
        {
        }

        public RequiredParameterException(string message) : base(message)
        {
        }

        public RequiredParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RequiredParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Key = info.GetValue(nameof(Key), typeof(string)) as string;
            Expiry = info.GetValue(nameof(Expiry), typeof(TimeSpan?)) as TimeSpan?;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Key), Key);
            info.AddValue(nameof(Expiry), Expiry.Value);
        }
    }
}