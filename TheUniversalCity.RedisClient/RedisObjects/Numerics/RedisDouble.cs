using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.Numerics
{
    public class RedisDouble : RedisObject<double>
    {
        public const byte DETERMINATIVE_CHAR = (byte)',';
        public const string POSITIVE_INF = "inf";
        public const string NEGATIVE_INF = "-inf";

        public static RedisDouble Parse(IEnumerator<byte> enumerator)
        {
            var str = ReadLineEofCrLf(enumerator, Encoding.ASCII);

            switch (str)
            {
                case POSITIVE_INF:
                    return new RedisDouble { Value = double.PositiveInfinity };
                case NEGATIVE_INF:
                    return new RedisDouble { Value = double.NegativeInfinity };
                default:
                    return new RedisDouble { Value = double.Parse(str) };
            }
        }

        public static implicit operator double(RedisDouble redisDouble)
        {
            return redisDouble.Value;
        }
    }
}
