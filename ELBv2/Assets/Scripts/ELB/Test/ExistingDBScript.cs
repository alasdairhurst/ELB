using UnityEngine;
using ELB.Models;
using BattleKit.Engine;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization
	public Cell cell;
	public Board board;
	public string otherScene;
	void Start() {
		//var save = SaveManager.GetLatestSave();
		//if (save != null) {
		//SaveManager.SetCurrentSave(save);
		//}
		board = Model.Find<Board>("New Board3");
		cell = ScriptableObject.CreateInstance<Cell>();
		cell.name = "hahaha";
		board = board.Clone();
		Debug.Log(cell.ToString(true));
		Debug.Log(board.ToString(true));
	}
}
