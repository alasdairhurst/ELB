using UnityEngine;
using ELB.Data.Models;
using ELB.Utils;
using ELB.Data.Collections;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
		var c = new Cell();
		Debug.Log(c);
		Debug.Log(c.ToString(StringOpts.Pretty));
		Board board = new Board();
		board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
		Debug.Log(board.ToString(StringOpts.Pretty));
		Debug.Log(board.ToString(StringOpts.Full));
		Debug.Log(board.ToString(StringOpts.Short));
		Debug.Log(board.ToString(StringOpts.OneLine));
		Debug.Log(board.ToString(StringOpts.TwoLine));

	}
}
