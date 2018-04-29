using BattleKit.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ELB.Models {
	public class Cell : Model {
		public int Size;
		public float Height;
		public Landscape Landscape;
		public string Name;
		public Building Building;
		public string Owner;
		public string[] Commanders;
		public List<Unit> Prisoners;
		public List<Unit> Defenders;

		public Cell() : base() {
		}

		public override void Init(JToken data) {
			base.Init(data);
			Size = data.Value<int>("size");
			Height = data.Value<float>("height");
			Name = data.Value<string>("name");
			Owner = data.Value<string>("owner");
		}

		public override JToken Serialize() {
			return new JObject(
				new JProperty("id", id),
				new JProperty("size", Size),
				new JProperty("height", Height),
				new JProperty("name", Name),
				new JProperty("owner", Owner)
			);
		}
	}
}
