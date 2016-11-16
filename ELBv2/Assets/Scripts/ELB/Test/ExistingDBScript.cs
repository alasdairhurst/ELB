using UnityEngine;
using ELB.Data.Models;
using ELB.Utils;
using UnityEngine.SceneManagement;
using ELB.Data.Helpers;
using System.Linq;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization

	public string otherScene;
	void Start() {

		var save = SaveManager.GetLatestSave();
		if (save != null) {
			SaveManager.SetCurrentSave(save);
		}

		Board board = new Board();
		board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
		Debug.Log(board.ToString(StringOpts.Pretty));

		if (save == null) {
			board.Name = "changedName";
			board.Save();
			save = SaveManager.CreateSave();
			GameState.Save(save);
			Debug.Log("Saving to " + save.Filename);
		} else {
			GameState.Load(save);
			board.Fetch("{9A69826B-5BC5-4F89-9066-6D52D598979B}");
			Debug.Log(board.ToString(StringOpts.Pretty));
		}
	}
}