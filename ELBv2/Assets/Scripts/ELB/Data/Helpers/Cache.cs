using System;
using System.Collections.Generic;

namespace ELB.Data.Helpers {
	public class Cache<K, V> : Dictionary<K, V> {

		private double timeToLive = double.PositiveInfinity;
		private Dictionary<K, long> storedTime;

		private long currentTime {
			get { return DateTime.Now.ToFileTime(); }
		}

		public Cache() {
			storedTime = new Dictionary<K, long>();
		}

		// set ttl in ms
		public void SetTTL(double ttl) {
			timeToLive = ttl;
		}

		public void SetOne(K key, V value) {
			storedTime[key] = currentTime;
			this[key] = value;
		}

		public T GetOne<T>(K key, T instance = default(T)) where T : V {
			if (!ContainsKey(key)) {
				return default(T);
			} else {
				V value;
				TryGetValue(key, out value);
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
						Remove(key);
						return default(T);
					}
				}
			}
		}

		public IEnumerable<T> Get<T>(IEnumerable<K> keys, T instance = default(T)) where T : V {
			var vals = new List<T>();
			foreach (K key in keys) {
				T val = GetOne<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public IEnumerable<T> GetAll<T>(T instance = default(T)) where T : V {
			var vals = new List<T>();
			foreach (K key in Keys) {
				T val = GetOne<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public new void Clear() {
			base.Clear();
			storedTime.Clear();
		}

	}
}
