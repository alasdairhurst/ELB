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
			storedTime[key] = currentTime;
			storedData[key] = value;
		}

		public List<T> get<T>(List<K> keys) where T : V {
			List<T> vals = new List<T>();
			foreach (K key in keys) {
				T val = get<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public List<T> get<T>() where T : V {
			List<T> vals = new List<T>();
			foreach (K key in storedData.Keys) {
				T val = get<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public T get<T>(K key) where T : V {
			if (!storedData.ContainsKey(key)) {
				return default(T);
			} else {
				V value;
				storedData.TryGetValue(key, out value);
				if (double.IsInfinity(timeToLive)) {
					return (T)(object)value;
				} else {
					long savedTime;
					storedTime.TryGetValue(key, out savedTime);
					if (savedTime + timeToLive <= currentTime) {
						if (value is T) {
							return (T)(object)value;
						}
						return default(T);
					} else {
						storedTime.Remove(key);
						storedData.Remove(key);
						return default(T);
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
