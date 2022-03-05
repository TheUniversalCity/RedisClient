using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.SimpleStrings
{
    public class RedisSimpleError : RedisObject<string>
    {
        public const byte DETERMINATIVE_CHAR = (byte)'-';

        public static RedisSimpleError Parse(IEnumerator<byte> enumerator)
        {
            return new RedisSimpleError { Value = ReadLineEofCrLf(enumerator, Encoding.UTF8) };
        }

        public static implicit operator string(RedisSimpleError redisSimpleString)
        {
            return redisSimpleString.Value;
        }
    }
}
