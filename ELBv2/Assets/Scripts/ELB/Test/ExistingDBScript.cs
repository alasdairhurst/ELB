using UnityEngine;
using ELB.Data.Models;
using ELB.Utils;
using ELB.Data.Collections;
using UnityEngine.SceneManagement;
using ELB.Data.Helpers;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization

	public string otherScene;
	void Start () {
		Board board = new Board();
		board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
		Debug.Log(board.ToString(StringOpts.Pretty));

		SaveManager.CreateSave();
		string[] saves = SaveManager.GetSaves();
		foreach (string save in saves) {
			Debug.Log(save);
		}

		if (board.Name != "boobies") {
			board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
			Debug.Log(board.Name);
			Debug.Log(board.ToString(StringOpts.Pretty));
			board.Name = "boobies";
			Debug.Log("Saving");
			board.SaveState();

			SceneManager.LoadScene(otherScene);
		}
	}
}
