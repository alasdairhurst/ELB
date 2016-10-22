using System;

namespace ELB.Data.Models {
	class Purchasable : Model {
		public Purchasable() : base() { }

		public override bool Fetch(string id) {
			return Fetch<Purchasable>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

		public string Name { get; set; }
		public int Cost { get; set; }
		public int Level { get; set; }
		public string Icon { get; set; }
		public string Type { get; set; }
		public string TypeId { get; set; }
	}
}
