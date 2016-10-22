using System;
using System.Collections.Generic;

namespace ELB.Data.Helpers {
	public class Cache<K, V> {

		private double timeToLive = double.PositiveInfinity;
		private Dictionary<K, long> storedTime;
		private Dictionary<K, V> storedData;

		private long currentTime {
			get { return DateTime.Now.ToFileTime(); }
		}

		public Cache() {
			storedData = new Dictionary<K, V>();
			storedTime = new Dictionary<K, long>();
		}

		// set ttl in ms
		public void setTimeToLive(double ttl) {
			timeToLive = ttl;
		}

		public void set(K key, V value) {
			storedTime.Add(key, currentTime);
			storedData.Add(key, value);
		}

		public List<V> get(List<K> keys) {
			List<V> vals = new List<V>();
			foreach (K key in keys) {
				V val = get(key);
				if (!EqualityComparer<V>.Default.Equals(val, default(V))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public List<V> get() {
			List<V> vals = new List<V>();
			foreach (K key in storedData.Keys) {
				V val = get(key);
				if (!EqualityComparer<V>.Default.Equals(val, default(V))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public V get(K key) {
			if (!storedData.ContainsKey(key)) {
				return default(V);
			} else {
				V value;
				storedData.TryGetValue(key, out value);
				if (double.IsInfinity(timeToLive)) {
					return value;
				} else {
					long savedTime;
					storedTime.TryGetValue(key, out savedTime);
					if (savedTime + timeToLive <= currentTime) {
						return value;
					} else {
						storedTime.Remove(key);
						storedData.Remove(key);
						return default(V);
					}
				}
			}
		}

		public void clear() {
			storedData.Clear();
			storedTime.Clear();
		}

	}
}
