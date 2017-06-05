using BattleKit.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace ELB.Models {
	public class Board : Model {
		public int CellSize;
		public int BoardSize;
		public string Name;
		[SerializeField]
		public List<Cell> Cells;
		public float Scale;
	}
}
