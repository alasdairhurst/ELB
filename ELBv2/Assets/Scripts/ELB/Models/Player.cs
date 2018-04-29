using BattleKit.Engine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ELB.Models {
	public enum something {
		test,
		no
	}
	public class Player : Model {
		public int TeamId;
		public List<Unit> Units;
		public int Money;
		public int Level;
		public bool Text;
		public something Something;

		public override void Init(JToken data) {
			base.Init(data);
			TeamId = data.Value<int>("teamid");
			Money = data.Value<int>("money");
			Level = data.Value<int>("level");
			Text = data.Value<bool>("text");
			Something = data.Value<something>("something");
		}

		public override JToken Serialize() {
			return new JObject(
				new JProperty("id", id),
				new JProperty("teamid", TeamId),
				new JProperty("money", Money),
				new JProperty("level", Level),
				new JProperty("text", Text),
				new JProperty("something", Something)
			);
		}

	}
}
