using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Building : Model {
		public Building() : base() { }

		public string Name { get; set; }
		public string Prefab { get; set; }

		public override bool Fetch(string id) {
			return Fetch<Building>(id);
		}

		public override bool LoadTemp(string id) {
			return LoadTemp<Building>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}
	}
}
