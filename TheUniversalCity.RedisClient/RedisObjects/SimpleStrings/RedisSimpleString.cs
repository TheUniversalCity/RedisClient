using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.SimpleStrings
{
    public class RedisSimpleString : RedisObject<string>
    {
        public const byte DETERMINATIVE_CHAR = (byte)'+';

        public static RedisSimpleString Parse(IEnumerator<byte> enumerator)
        {
            return new RedisSimpleString { Value = ReadLineEofCrLf(enumerator, Encoding.UTF8) };
        }

        public static implicit operator string(RedisSimpleString redisSimpleString)
        {
            return redisSimpleString.Value;
        }
    }
}
