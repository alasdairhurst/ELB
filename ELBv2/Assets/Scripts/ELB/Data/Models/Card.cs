using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	class Card : Model {
		public Card() : base() { }

		protected override void Initialise<T>(T model) {
		}

		public override bool Fetch(string id) {
			return Fetch<Card>(id);
		}
		public override bool LoadTemp(string id) {
			return LoadTemp<Card>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}
	}
}
