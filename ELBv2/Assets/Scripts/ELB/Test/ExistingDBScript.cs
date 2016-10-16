using UnityEngine;
using ELB.Data.Models;
using ELB.Data.Collections;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization
	void Start () {

		Board board = new Board();
		board.Fetch(2352);
		Debug.Log(board.ToString(ELB.Utils.StringOpts.Pretty));
		Debug.Log(board.ToString(ELB.Utils.StringOpts.Full));
		Debug.Log(board.ToString(ELB.Utils.StringOpts.Short));
		Debug.Log(board.ToString(ELB.Utils.StringOpts.OneLine));
		Debug.Log(board.ToString(ELB.Utils.StringOpts.TwoLine));
		Debug.Log(board);
		Debug.Log(board.Cells);
		Collection<Cell> cells = new Collection<Cell>("1,2,3,4,5");
		Debug.Log(cells);

	}
}
