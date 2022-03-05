using System.Collections.Generic;

namespace TheUniversalCity.RedisClient.RedisObjects
{
    public class RedisEndType : RedisObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'.';

        public static RedisEndType Parse(IEnumerator<byte> enumerator)
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

            return new RedisEndType();
        }

        public override string ToString()
        {
            return "End Type";
        }
    }
}
