using System.Collections.Generic;
using TheUniversalCity.RedisClient.RedisObjects;
using TheUniversalCity.RedisClient.RedisObjects.Agregates;
using TheUniversalCity.RedisClient.RedisObjects.Agregates.Abstract;
using TheUniversalCity.RedisClient.RedisObjects.BlobStrings;
using TheUniversalCity.RedisClient.RedisObjects.Numerics;
using TheUniversalCity.RedisClient.RedisObjects.SimpleStrings;

namespace TheUniversalCity.RedisClient
{
    public static class RedisObjectDeterminator
    {
        public const byte CR = (byte)'\r';
        public const byte LF = (byte)'\n';

        public static RedisObject Determine(IEnumerator<byte> enumerator)
        {
            enumerator.MoveNext();

            var determinativeChar = enumerator.Current;

            switch (determinativeChar)
            {
                case RedisArray.DETERMINATIVE_CHAR:
                    return RedisCollectionObject.GetRedisCollectionObject<RedisArray>(enumerator);
                case RedisAttributeType.DETERMINATIVE_CHAR:
                    {
                        var attribute = RedisDictionaryObject.GetRedisDictionaryObject<RedisAttributeType>(enumerator); ;
                        var afterObj = Determine(enumerator);

                        afterObj.SetAttribute(attribute);

                        return afterObj;
                    }
                case RedisBigNumber.DETERMINATIVE_CHAR:
                    return RedisBigNumber.Parse(enumerator);
                case RedisBlobError.DETERMINATIVE_CHAR:
                    return RedisBlobError.Parse(enumerator);
                case RedisBlobString.DETERMINATIVE_CHAR:
                    return RedisBlobString.Parse(enumerator);
                case RedisBoolean.DETERMINATIVE_CHAR:
                    return RedisBoolean.Parse(enumerator);
                case RedisDouble.DETERMINATIVE_CHAR:
                    return RedisDouble.Parse(enumerator);
                case RedisEndType.DETERMINATIVE_CHAR:
                    return RedisEndType.Parse(enumerator);
                case RedisMapType.DETERMINATIVE_CHAR:
                    return RedisDictionaryObject.GetRedisDictionaryObject<RedisMapType>(enumerator);
                case RedisNull.DETERMINATIVE_CHAR:
                    return RedisNull.Parse(enumerator);
                case RedisNumber.DETERMINATIVE_CHAR:
                    return RedisNumber.Parse(enumerator);
                case RedisPushType.DETERMINATIVE_CHAR:
                    return RedisCollectionObject.GetRedisCollectionObject<RedisPushType>(enumerator);
                case RedisSetReply.DETERMINATIVE_CHAR:
                    return RedisCollectionObject.GetRedisCollectionObject<RedisSetReply>(enumerator);
                case RedisSimpleError.DETERMINATIVE_CHAR:
                    return RedisSimpleError.Parse(enumerator);
                case RedisSimpleString.DETERMINATIVE_CHAR:
                    return RedisSimpleString.Parse(enumerator);
                case RedisVerbatimString.DETERMINATIVE_CHAR:
                    return RedisVerbatimString.Parse(enumerator);
                default:
                    return null;
            }
        }
    }
}
