using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.Agregates.Abstract
{
    public abstract class RedisDictionaryObject : RedisObject, IReadOnlyDictionary<RedisObject, RedisObject>, IEnumerable<KeyValuePair<RedisObject, RedisObject>>
    {
        public RedisObject this[RedisObject key] => Dictionary[key];

        private Dictionary<RedisObject, RedisObject> Dictionary { get; set; }

        public IEnumerable<RedisObject> Keys => Dictionary.Keys;

        public IEnumerable<RedisObject> Values => Dictionary.Values;

        public int Count => Dictionary.Count;

        public static TRedisDictionaryObject GetRedisDictionaryObject<TRedisDictionaryObject>(IEnumerator<byte> enumerator
#if DEBUG
                                                                                              ,
                                                                                              System.Action<string> logger
#endif
        ) where TRedisDictionaryObject : RedisDictionaryObject, new() {
            var length = int.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII));
            var collectionObject = new TRedisDictionaryObject();

            if (length == -1) {
                return collectionObject;
            }

            collectionObject.Dictionary = new Dictionary<RedisObject, RedisObject>(length);

            for (int i = 0; i < length; i++) {
                collectionObject.Dictionary.Add(
                    RedisObjectDeterminator.Determine(
                        enumerator
#if DEBUG
                        ,
                        logger
#endif
                    ),
                    RedisObjectDeterminator.Determine(
                        enumerator
#if DEBUG
                        ,
                        logger
#endif
                    )
                );
            }

            return collectionObject;
        }

        public bool ContainsKey(RedisObject key)
        {
            return Dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<RedisObject, RedisObject>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public bool TryGetValue(RedisObject key, out RedisObject value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }
    }
}
