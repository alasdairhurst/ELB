using BattleKit.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ELB.Models {
	public class Board : Model {
		public int CellSize;
		public int BoardSize;
		public string Name;
		public List<Cell> Cells;
		public float Scale;

		public Board() : base() {
			Cells = new List<Cell>();
		}

		public override void Init(JToken data) {
			base.Init(data);
			CellSize = data.Value<int>("cellSize");
			BoardSize = data.Value<int>("boardSize");
			Name = data.Value<string>("name");
			var cells = data["cells"].ToObject<List<JToken>>()
				.Select(c => {
					var x = CreateInstance<Cell>();
					x.Init(data);
					return x;
				});
			Cells = new List<Cell>(cells);
			Scale = data.Value<float>("scale");
		}

		public override JToken Serialize() {
			return new JObject(
				new JProperty("id", id),
				new JProperty("cellSize", CellSize),
				new JProperty("boardSize", BoardSize),
				new JProperty("name", Name),
				new JProperty("cells",
					new JArray(
						from c in Cells
						select c.Serialize()
					)
				),
				new JProperty("scale", Scale)
			);
		}
	}
}
