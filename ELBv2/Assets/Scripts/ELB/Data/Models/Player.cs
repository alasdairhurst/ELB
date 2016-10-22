using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	class Player : Model {
		public Player() : base() { }

		public int TeamId { get; set; }
		public string UnitIds { get; set; }
		public Collection<Unit> Units { get; set; }
		public int Money { get; set; }
		public int Level { get; set; }


		protected override void Initialise<T>(T model) {
		}

		public override bool Fetch(string id) {
			return Fetch<Player>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

	}
}
