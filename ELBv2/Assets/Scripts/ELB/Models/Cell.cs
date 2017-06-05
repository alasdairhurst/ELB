using BattleKit.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace ELB.Models {
	public class Cell : Model {
		[SerializeField]
		public int Size;
		[SerializeField]
		public float Height;
		[SerializeField]
		public Landscape Landscape;
		[SerializeField]
		public string Name;
		[SerializeField]
		public Building Building;
		[SerializeField]
		public string Owner;
		[SerializeField]
		public string[] Commanders;
		[SerializeField]
		public List<Unit> Prisoners;
		[SerializeField]
		public List<Unit> Defenders;
	}
}
