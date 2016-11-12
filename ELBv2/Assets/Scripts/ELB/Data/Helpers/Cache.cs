using System;
using System.Collections.Generic;
using ELB.Data.Collections;
using ELB.Data.Models;

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
		public void SetTTL(double ttl) {
			timeToLive = ttl;
		}

		public void SetOne(K key, V value) {
			storedTime[key] = currentTime;
			storedData[key] = value;
		}

		public T GetOne<T>(K key) where T : V {
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

		public IEnumerable<T> Get<T>(IEnumerable<K> keys) where T : V {
			var vals = new List<T>();
			foreach (K key in keys) {
				T val = GetOne<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public IEnumerable<T> GetAll<T>() where T : V {
			var vals = new List<T>();
			foreach (K key in storedData.Keys) {
				T val = GetOne<T>(key);
				if (!EqualityComparer<T>.Default.Equals(val, default(T))) {
					vals.Add(val);
				}
			}
			return vals;
		}

		public void Clear() {
			storedData.Clear();
			storedTime.Clear();
		}

	}
}
