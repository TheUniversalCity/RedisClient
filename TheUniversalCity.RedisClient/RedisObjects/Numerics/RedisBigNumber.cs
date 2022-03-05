using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.Numerics
{
    public class RedisBigNumber: RedisObject<string>
    {
        public const byte DETERMINATIVE_CHAR = (byte)'(';

        public static RedisBigNumber Parse(IEnumerator<byte> enumerator)
        {
            return new RedisBigNumber { Value = ReadLineEofCrLf(enumerator, Encoding.ASCII) };
        }

        public static implicit operator string(RedisBigNumber redisBigNumber)
        {
            return redisBigNumber.Value;
        }
    }
}
