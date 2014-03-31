using System;
using System.Collections.Generic;

namespace Awagat.OneDrive.Util
{
    public class Cache<X, Y>
    {
        #region Helper types

        private class Cached<T>
        {
            #region Constants

            public readonly TimeSpan DefaultTimeToLive = new TimeSpan(0, 0, 5);

            #endregion

            #region Properties

            public T Item
            {
                get;
                private set;
            }

            public DateTimeOffset Expiration
            {
                get;
                private set;
            }

            public bool IsExpired
            {
                get
                {
                    return DateTimeOffset.UtcNow > Expiration;
                }
            }

            #endregion

            #region .ctor

            public Cached(T item, TimeSpan? timeToLive = null)
            {
                Item = item;
                Expiration = DateTimeOffset.UtcNow.Add(timeToLive.HasValue ? timeToLive.Value : DefaultTimeToLive);
            }

            #endregion
        }

        #endregion

        #region Private member variables

        private Dictionary<X, Cached<Y>> cache = new Dictionary<X, Cached<Y>>();

        #endregion

        #region Public methods

        public Y Add(X key, Y item)
        {
            if (cache.ContainsKey(key))
                cache.Remove(key);

            cache.Add(key, new Cached<Y>(item));

            return item;
        }

        public Y Get(X key)
        {
            Cached<Y> cached = null;

            if (!cache.TryGetValue(key, out cached) || cached.IsExpired)
                return default(Y);

            return cached.Item;
        }

        public void Invalidate(X key)
        {
            if (cache.ContainsKey(key))
                cache.Remove(key);
        }

        #endregion
    }
}
