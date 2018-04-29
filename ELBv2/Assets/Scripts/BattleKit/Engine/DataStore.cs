using ELB.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BattleKit.Engine {

	class DataStoreMetadata : JObject {
		public DataStoreMetadata() {
			Add("name", "");
			Add("lastModified", "");
			Add("author", "");
			Add("version", "");
			Add("dependencies", new JArray());
		}
	}

	class DataStoreData : JObject {
		public DataStoreData() {
			Add("boards", new JObject());
		}
	}

	class DataStore : JObject {
		public DataStore() {
			Add("metadata", new DataStoreMetadata());
			Add("ids", new JObject());
			Add("data", new DataStoreData());
		}
	}
}
