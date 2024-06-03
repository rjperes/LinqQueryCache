using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Linq.Expressions;

namespace LinqQueryCache
{
    sealed class QueryableWrapper<T> : IOrderedQueryable<T>, IDisposable
    {
        private static readonly ExpressionEqualityComparer _comparer = new ExpressionEqualityComparer();

        sealed class EnumeratorWrapper : IEnumerator<T>
        {
            private readonly LinkedList<T> _list = new LinkedList<T>();
            private QueryableWrapper<T> _queryable;
            private IEnumerator<T> _enumerator;
            private bool _stored = false;
            internal bool _consumed;

            public EnumeratorWrapper(QueryableWrapper<T> queryable, IEnumerator<T> enumerator)
            {
                this._enumerator = enumerator;
                this._queryable = queryable;
            }

            internal IEnumerator<T> FromCache()
            {
                return this._list.GetEnumerator();
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get
                {
                    var current = this._enumerator.Current;

                    if (!this._stored)
                    {
                        this._list.AddLast(current);
                        this._stored = true;
                    }

                    return current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                this._stored = false;
                this._consumed = true;
                this._enumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
                    return this.Current!;
                }
            }

            public bool MoveNext()
            {
                var result = this._enumerator.MoveNext();

                if (result)
                {
                    this._stored = false;
                }

                return result;
            }

            public void Reset()
            {
                this._stored = false;
                this._list.Clear();
                this._enumerator.Reset();
            }

            #endregion
        }

        #region Private readonly fields
        private readonly IQueryable<T> _queryable;
        private readonly IMemoryCache _cache;
        private readonly int _durationSeconds;
        #endregion

        #region Internal constructor
        internal QueryableWrapper(IMemoryCache cache, IQueryable<T> queryable, int durationSeconds = -1)
        {
            this._cache = cache;
            this._queryable = queryable;
            this._durationSeconds = durationSeconds;
        }
        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> enumerator;
            var key = this.GetKey(this._queryable);

            if (this._cache.TryGetValue(key, out var value) && (value is EnumeratorWrapper wrapper))
            {
                //hit
                QueryCache.RaiseHit(this._queryable);
                enumerator = wrapper;
                if (wrapper._consumed)
                {
                    return wrapper.FromCache();
                }
            }
            else
            {
                //miss
                QueryCache.RaiseMiss(this._queryable);
                enumerator = new EnumeratorWrapper(this, this._queryable.GetEnumerator());
                if (this._durationSeconds > 0)
                {
                    //do not store, only return from cache if it's there
                    this._cache.Set(key, enumerator, DateTimeOffset.Now.AddSeconds(this._durationSeconds));
                }
            }

            return enumerator;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get
            {
                return this._queryable.ElementType;
            }
        }

        public Expression Expression
        {
            get
            {
                return this._queryable.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return this._queryable.Provider;
            }
        }

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            var key = this.GetKey(this._queryable);

            this._cache.Remove(key);
        }
        #endregion

        #region Private methods
        private int GetKey(IQueryable queryable)
        {
            return _comparer.GetHashCode(queryable.Expression);
        }
        #endregion
    }
}
