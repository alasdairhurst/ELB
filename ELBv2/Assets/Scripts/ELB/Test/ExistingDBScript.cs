using UnityEngine;
using ELB.Models;
using BattleKit.Engine;
using Newtonsoft.Json.Linq;
using JsonDiffPatchDotNet;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;

public class ExistingDBScript : MonoBehaviour {
	// Use this for initialization
	public DataManager dataManager;
	public Cell cell;
	public Board board;
	public string otherScene;
	void Start() {
		//var save = SaveManager.GetLatestSave();
		//if (save != null) {
		//SaveManager.SetCurrentSave(save);
		//}

		dynamic json = JObject.Parse(@"{
			'test': {
				'foo': 'bar',
				'bar': 123,
				'baz': [
					1,
					'hello',
				]
			}
		}");

		dynamic patch = JObject.Parse(@"{
			'test': {
				'foo': [
					'bar',
					'baz'
				],
				'baz': {
					'_t': 'a',
					'2': [
						true
					]
				}
			}
		}");

		Debug.Log(json.test.baz[1]); // hello

		var jdp = new JsonDiffPatch();

		dynamic json2 = jdp.Patch(json, patch);

		Debug.Log(json2.ToString());

		json2.test.baz[2] = null;

		JToken newDiff = jdp.Diff(json, json2);

		Debug.Log(newDiff.ToString());

		var boards = new Dictionary<string, int>();

		boards.Add("foo", 1);
		boards.Add("bar", 2);

		var x = new JObject(
			from b in boards
			select new JProperty(b.Key, b.Value)
		);
		Debug.Log(x.ToString());

		if (dataManager.loaded) {
			dataManager.loaded = false;
			dataManager.OnEnable();
		}

		Cell c = dataManager.CreateModel<Cell>();
		c.name = "cell_1";
		c.Height = 32;
		c.Name = "cell1";

		Board bo = dataManager.CreateModel<Board>();
		bo.name = "board_1";
		bo.Scale = 12.23f;
		bo.Cells.Add(c);
		bo.Cells.Add(c);
		bo.CellSize = 3;

		foreach (string file in Directory.GetFiles(Application.persistentDataPath, "*.sav")) {
			dataManager.LoadData(file, FileType.Save, true);
			//dataManager.LoadSave(Path.GetFileName(file));
		}

		dataManager.UpdateModel(bo);
		dataManager.UpdateModel(c);

		dataManager.CreateSave("alasdair", "testfile", "1");

		/*
			board = Model.Find<Board>("New Board3");
			cell = ScriptableObject.CreateInstance<Cell>();
			cell.name = "hahaha";
			cell.Owner = "foo";
			board = board.Clone();
			Debug.Log(cell.ToString(true));
			Debug.Log(board.ToString(true));
		*/

	}
}
