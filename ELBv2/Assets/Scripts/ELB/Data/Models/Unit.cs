using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	class Unit : Actor {
		public Unit() : base() { }

		protected override void Initialise<T>(T model) {
		}

		public override bool Fetch(string id) {
			return Fetch<Unit>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}
	}
}
