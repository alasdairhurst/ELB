using System.Linq;
using System.Collections.Generic;

namespace Engine.Data {
	public class Cache<K, V> : Dictionary<K, V> {

		private double timeToLive = double.PositiveInfinity;
		private Dictionary<K, long> storedTime;

		private long currentTime {
			get { return System.DateTime.Now.ToFileTime(); }
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
			}
			V value;
			TryGetValue(key, out value);
			if (!double.IsInfinity(timeToLive)) {
				long savedTime;
				storedTime.TryGetValue(key, out savedTime);
				if (savedTime + timeToLive > currentTime) {
					storedTime.Remove(key);
					Remove(key);
					return default(T);
				}
			}
			if (!(value is T)) {
				throw new System.Exception("Key does not correspond to specified value type");
			}
			return (T)(object)value;
			
		}

		public IEnumerable<T> Get<T>(IEnumerable<K> keys, T instance = default(T)) where T : V {
			var vals = new List<T>();
			foreach (K key in Keys) {
				if (keys.Contains(key) && this[key] is T) {
					T val = GetOne<T>(key);
					if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
						vals.Add(val);
					}
				}
			}
			return vals;
		}

		public IEnumerable<T> GetAll<T>(T instance = default(T)) where T : V {
			var vals = new List<T>();
			foreach (K key in Keys) {
				if (this[key] is T) {
					T val = GetOne<T>(key);
					if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
						vals.Add(val);
					}
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
