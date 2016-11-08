using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Landscape : Model {
		public Landscape() : base() { }

		public string Name { get; set; }
		public string Prefab { get; set; }

		public override bool Fetch(string id) {
			return Fetch<Landscape>(id);
		}
		public override bool LoadTemp(string id) {
			return LoadTemp<Landscape>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}
	}
}
