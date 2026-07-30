using System;
using System.Collections.Generic;

namespace ZNet.Communication
{
    public class NetObservers
    {
        private HashSet<int> _observers = new();

        private int[] _cachedArray = Array.Empty<int>();
        private bool _dirty = true;

        public event Action<int> OnObserverAdded;
        public event Action<int> OnObserverRemoved;
        public event Action OnObserversChanged;
        public int Count => _observers.Count;

        public void Add(int peerId)
        {
            if (_observers.Add(peerId))
            {
                _dirty = true;
                OnObserverAdded?.Invoke(peerId);
                OnObserversChanged?.Invoke();
            }
        }

        public void Remove(int peerId)
        {
            if (_observers.Remove(peerId))
            {
                _dirty = true;
                OnObserverRemoved?.Invoke(peerId);
                OnObserversChanged?.Invoke();
            }
        }

        public bool Contains(int peerId)
        {
            return _observers.Contains(peerId);
        }

        public int[] GetObserversArray()
        {
            if (_dirty)
            {
                _cachedArray = new int[_observers.Count];
                _observers.CopyTo(_cachedArray);
                _dirty = false;
            }
            return _cachedArray;
        }

        public void Clear()
        {
            _observers.Clear();
            _dirty = true;
            OnObserversChanged?.Invoke();
        }

        public void Recalculate(Func<int, bool> visibilityCheck, IEnumerable<int> allPeerIds)
        {
            _observers.Clear();

            foreach (var peerId in allPeerIds)
            {
                if (visibilityCheck(peerId))
                {
                    _observers.Add(peerId);
                }
            }

            _dirty = true;
            OnObserversChanged?.Invoke();
        }



    }
}