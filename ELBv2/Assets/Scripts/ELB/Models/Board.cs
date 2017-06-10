using BattleKit.Engine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ELB.Models {
	[Serializable]
	public class Board : Model {
		public int CellSize;
		public int BoardSize;
		public string Name;
		[SerializeField]
		public List<Cell> Cells;
		public float Scale;
	}
}
