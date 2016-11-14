using UnityEngine;
using ELB.Data.Models;
using ELB.Utils;
using UnityEngine.SceneManagement;
using ELB.Data.Helpers;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization

	public string otherScene;
	void Start() {
		Board board = new Board();
		board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
		Debug.Log(board.ToString(StringOpts.Pretty));

		board.Name = null;
		if (board.Name != "boobies") {
			board.Name = "boobies";
			board.SaveState();
			var newSave = SaveManager.CreateSave();
			Debug.Log("Saving to " + newSave.Filename);
			//GameState.Save(newSave);
			SceneManager.LoadScene(otherScene);
		} else {
			var save = SaveManager.GetLatestSave();
			if (save != null) {
				Debug.Log("Loading Save " + save.Filename);
				SaveManager.SetCurrentSave(save);
				SaveManager.LoadData();
			}
		}
	}
}