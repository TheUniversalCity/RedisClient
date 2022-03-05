using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUniversalCity.RedisClient.RedisObjects.BlobStrings.Abstract;

namespace TheUniversalCity.RedisClient.RedisObjects.BlobStrings
{
    public class RedisBlobString : RedisBlobObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'$';

        public static RedisBlobString Parse(IEnumerator<byte> enumerator)
        {
            var length = int.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII));
#if DEBUG
            Console.WriteLine($"RedisBlobString Parse : Length =>{length}");
#endif
            if (length == -1)
            {
                return new RedisBlobString();
            }

            return new RedisBlobString { Values = ReadBlobEofCrLf(enumerator, length, Encoding.UTF8) };
        }

        public static implicit operator string(RedisBlobString redisBlobString)
        {
            return redisBlobString.Values?.FirstOrDefault();
        }
    }
}
