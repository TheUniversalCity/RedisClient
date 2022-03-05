using System;
using System.Collections.Generic;

namespace TheUniversalCity.RedisClient.RedisObjects
{
    public class RedisBoolean : RedisObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'#';
        public const byte TRUE_CHAR = (byte)'t';
        public const byte FALSE_CHAR = (byte)'f';

        public bool Value { get; set; }

        public static RedisBoolean Parse(IEnumerator<byte> enumerator)
        {
            enumerator.MoveNext();

            var idByte = enumerator.Current;

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

            return new RedisBoolean { Value = idByte == TRUE_CHAR || (idByte == FALSE_CHAR ? false :  throw new InvalidOperationException()) };
        }

        public static implicit operator bool(RedisBoolean redisBoolean)
        {
            return redisBoolean.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
