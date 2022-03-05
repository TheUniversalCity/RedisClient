using System.Collections.Generic;

namespace TheUniversalCity.RedisClient.RedisObjects
{
    public class RedisNull : RedisObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'_';

        public static RedisNull Parse(IEnumerator<byte> enumerator)
        {
            enumerator.MoveNext(); // CR

            //if (enumerator.Current != RedisObjectDeterminator.CR)
            //{
            //    throw new InvalidOperationException();
            //}

            enumerator.MoveNext(); // LF

            //if (enumerator.Current != RedisObjectDeterminator.LF)
            //{
            //    throw new InvalidOperationException();
            //}

            return new RedisNull();
        }

        public override string ToString()
        {
            return "Null";
        }
    }
}
