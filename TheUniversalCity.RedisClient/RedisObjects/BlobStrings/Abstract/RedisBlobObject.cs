namespace TheUniversalCity.RedisClient.RedisObjects.BlobStrings.Abstract
{
    public abstract class RedisBlobObject : RedisObject
    {
        public string[] Values { get; protected set; }

        public override string ToString()
        {
            return Values?[0];
        }
    }

}
