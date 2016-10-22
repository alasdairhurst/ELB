using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	class Actor : Model {
		public Actor() : base() { }

		protected override void Initialise<T>(T model) {
		}

		public override bool Fetch(string id) {
			return Fetch<Actor>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}
	}
}
