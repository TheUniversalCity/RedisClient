using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TheUniversalCity.RedisClient.RedisObjects.Agregates.Abstract
{
    public abstract class RedisCollectionObject : RedisObject, ICollection<RedisObject>
    {
        public RedisObject this[int key] => Items[key];
        public List<RedisObject> Items { get; set; }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public static TRedisCollectionObject GetRedisCollectionObject<TRedisCollectionObject>(IEnumerator<byte> enumerator) where TRedisCollectionObject : RedisCollectionObject, new()
        {
            var length = int.Parse(ReadLineEofCrLf(enumerator, Encoding.ASCII));
            var collectionObject = new TRedisCollectionObject();

            if (length == -1)
            {
                return collectionObject;
            }

            collectionObject.Items = new List<RedisObject>(length);

            for (int i = 0; i < length; i++)
            {
                collectionObject.Add(RedisObjectDeterminator.Determine(enumerator));
            }

            return collectionObject;
        }

        public void Add(RedisObject item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(RedisObject item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(RedisObject[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<RedisObject> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public bool Remove(RedisObject item)
        {
            return Items.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
