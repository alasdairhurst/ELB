using UnityEngine;
using ELB.Data.Models;
using ELB.Utils;


public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization
	void Start () {

		Board board = new Board();
		board.Fetch(2352);
		Debug.Log(board.ToString(StringOpts.Pretty));
		Debug.Log(board.ToString(StringOpts.Full));
		Debug.Log(board.ToString(StringOpts.Short));
		Debug.Log(board.ToString(StringOpts.OneLine));
		Debug.Log(board.ToString(StringOpts.TwoLine));

	}
}
