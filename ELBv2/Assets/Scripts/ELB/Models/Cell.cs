using BattleKit.Engine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ELB.Models {
	[Serializable]
	public class Cell : Model {
		[SerializeField]
		private int Size;
		[SerializeField]
		private float Height;
		[SerializeField]
		private Landscape Landscape;
		[SerializeField]
		private string Name;
		[SerializeField]
		private Building Building;
		[SerializeField]
		private string Owner;
		[SerializeField]
		private string[] Commanders;
		[SerializeField]
		private List<Unit> Prisoners;
		[SerializeField]
		private List<Unit> Defenders;
	}
}
