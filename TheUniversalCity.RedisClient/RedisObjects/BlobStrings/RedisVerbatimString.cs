using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUniversalCity.RedisClient.RedisObjects.BlobStrings.Abstract;

namespace TheUniversalCity.RedisClient.RedisObjects.BlobStrings
{
    public class RedisVerbatimString : RedisBlobObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'=';
        public const byte COLON = (byte)':';

        public string Prefix { get; set; }

        public static RedisVerbatimString Parse(IEnumerator<byte> enumerator)
        {
            var length = long.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII));

            if (length == -1)
            {
                return new RedisVerbatimString();
            }

            var bytesPrefix = new byte[3];

            enumerator.MoveNext();
            bytesPrefix[0] = enumerator.Current;

            enumerator.MoveNext();
            bytesPrefix[1] = enumerator.Current;

            enumerator.MoveNext();
            bytesPrefix[2] = enumerator.Current;

            enumerator.MoveNext(); // : character

            if (enumerator.Current != COLON) { throw new InvalidOperationException(); }

            return new RedisVerbatimString { 
                Prefix = Encoding.ASCII.GetString(bytesPrefix), 
                Values = ReadBlobEofCrLf(enumerator, length - 4, Encoding.UTF8)
            };
        }

        public static implicit operator string(RedisVerbatimString redisVerbatimString)
        {
            return redisVerbatimString.Values.FirstOrDefault();
        }
    }
}
