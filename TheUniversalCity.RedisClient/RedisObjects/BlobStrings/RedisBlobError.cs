using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheUniversalCity.RedisClient.RedisObjects.BlobStrings.Abstract;

namespace TheUniversalCity.RedisClient.RedisObjects.BlobStrings
{
    public class RedisBlobError : RedisBlobObject
    {
        public const byte DETERMINATIVE_CHAR = (byte)'!';

        public static RedisBlobError Parse(IEnumerator<byte> enumerator)
        {
            var length = long.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII));

            if (length == -1)
            {
                return new RedisBlobError();
            }

            return new RedisBlobError { Values = ReadBlobEofCrLf(enumerator, length, Encoding.UTF8) };
        }

        public static implicit operator string(RedisBlobError redisBlobError)
        {
            return redisBlobError.Values?.FirstOrDefault();
        }
    }
}
