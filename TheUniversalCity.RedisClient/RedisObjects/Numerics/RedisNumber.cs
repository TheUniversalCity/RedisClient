using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.Numerics
{
    public class RedisNumber : RedisObject<long>
    {
        public const byte DETERMINATIVE_CHAR = (byte)':';

        public static RedisNumber Parse(IEnumerator<byte> enumerator)
        {
            return new RedisNumber { Value = long.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII)) };
        }

        public static implicit operator long(RedisNumber redisNumber)
        {
            return redisNumber.Value;
        }
    }
}
