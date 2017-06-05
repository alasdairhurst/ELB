using UnityEngine;
using ELB.Models;
using BattleKit.Engine;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization

	public string otherScene;
	void Start() {
		//var save = SaveManager.GetLatestSave();
		//if (save != null) {
			//SaveManager.SetCurrentSave(save);
		//}

		Board board = ScriptableObject.CreateInstance<Board>();
		Debug.Log(board.ToString(true));
		Debug.Log(board.ToString());
	}
}
