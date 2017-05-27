using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyReadOnlyList
{
    public class LazyReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly object _lock = new object();
        private readonly IEnumerator<T> _source;
        private readonly List<T> _enumeratedElements = new List<T>();
        private bool _elementsRemain = true;
        private int _enumeratedCount = 0;

        public LazyReadOnlyList(IEnumerable<T> source)
        {
            _source = source?.GetEnumerator() ?? throw new ArgumentNullException(nameof(source));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get {
                while (MoveNext()) {}
                return _enumeratedCount;
            }
        }

        public T this[int index]
        {
            get {
                if (index < 0) throw new ArgumentException("Argument cannot be negative", nameof(index));
                if (index >= _enumeratedCount) {
                    lock (_lock) {
                        while (index >= _enumeratedCount && _elementsRemain) {
                            MoveNext();
                        }
                    }
                    if (index >= _enumeratedCount) throw new Exception($"Index {index} is out of range");
                }
                return _enumeratedElements[index];
            }
        }

        private bool MoveNext()
        {
            if (!_elementsRemain) return false;

            _elementsRemain = _source.MoveNext();
            if (_elementsRemain) {
                _enumeratedElements.Add(_source.Current);
                _enumeratedCount++;
            }
            return _elementsRemain;
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly LazyReadOnlyList<T> _parent;
            private bool _elementsRemain = true;
            private int _index = 0;

            public Enumerator(LazyReadOnlyList<T> parent)
            {
                _parent = parent;
            }

            public void Dispose() {}

            public bool MoveNext()
            {
                if (!_elementsRemain) return false;

                if (_index >= _parent._enumeratedCount) {
                    lock (_parent._lock) {
                        if (_index >= _parent._enumeratedCount) {
                            _elementsRemain = _parent.MoveNext();
                        }
                    }
                }

                if (_elementsRemain) {
                    Current = _parent._enumeratedElements[_index];
                    _index++;
                }
                return _elementsRemain;
            }

            public void Reset()
            {
                _index = 0;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;
        }

    }
}
