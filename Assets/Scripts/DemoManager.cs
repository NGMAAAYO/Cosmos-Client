using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Defective.JSON;
using Michsky.MUIP;

public class DemoManager : MonoBehaviour
{
	public float playSpeed = 1.0f;
	public SliderManager progressBar;
	public RadialSlider speedController;
	public CustomDropdown demoList;
	public GameObject demoPanel;

	private JSONObject demo;
	private int currentRound = 0;
	private Texture2D maskTex;
	private bool paused = true;
	private string[] demoNames;
	private bool isPlaying = false;
	private GameObject map;

	public JSONObject readDemoFile(string FilePath) {
		if (!File.Exists(FilePath)) {
			Debug.LogError("File does not exist");
			return null;
		}
		string readData;
		using (StreamReader sr = File.OpenText(FilePath))
		{
			readData = sr.ReadToEnd();
			sr.Close();
		}
		if (readData == null) {
			Debug.LogError("File is empty");
			return null;
		}
		return new JSONObject(readData);
	}

	public void readDemo(string demoName) {
		demo = readDemoFile(Application.dataPath + "/Replays/" + demoName + ".rpl");
	}

	public void getAllDemoNames() {
		string[] filePaths = Directory.GetFiles(Application.dataPath + "/Replays/", "*.rpl");
		demoNames = new string[filePaths.Length];
		for (int i = 0; i < filePaths.Length; i++) {
			demoNames[i] = Path.GetFileNameWithoutExtension(filePaths[i]);
		}
	}

	public void prepareMap() {
		GameObject map_prefab = Resources.Load("Prefabs/map") as GameObject;
		map = Instantiate(map_prefab);
		map.name = "Map";
		int boardWidth = (int)demo["map"]["size"][0].floatValue;
		int boardHeight = (int)demo["map"]["size"][1].floatValue;
		maskTex = new Texture2D(boardWidth, boardHeight, TextureFormat.RFloat, false);
		maskTex.filterMode = FilterMode.Point;
		for (int y = 0; y < boardHeight; y++){
			for (int x = 0; x < boardWidth; x++){
				float value = demo["map"]["content"][x][y].floatValue;
				Color col = new Color(value, value, value, 1);
				maskTex.SetPixel(x, y, col);
			}
		}
		maskTex.Apply();
		Material mat = map.GetComponent<SpriteRenderer>().material;
		mat.SetTexture("_MaskTex", maskTex);
		mat.SetVector("_BoardSize", new Vector4(boardWidth, boardHeight, 0, 0));
		map.GetComponent<SpriteRenderer>().size = new Vector2(boardWidth, boardHeight);
		map.transform.localScale = new Vector2(boardWidth, boardHeight);
	}

	public void deleteMap() {
		Destroy(map);
		foreach (GameObject entity_obj in GameObject.FindGameObjectsWithTag("Entity")) {
			Destroy(entity_obj);
		}
	}

	public Vector3 transformLocation(JSONObject location){
		return new Vector3(
			location[0].floatValue - demo["map"]["origin"][0].floatValue - demo["map"]["size"][0].floatValue / 2f + 0.5f, 
			location[1].floatValue - demo["map"]["origin"][1].floatValue - demo["map"]["size"][1].floatValue / 2f + 0.5f, 
			0);
	}

	public GameObject addEntity(JSONObject entity) {
		GameObject entity_prefab = Resources.Load("Prefabs/entity") as GameObject;
		GameObject entity_obj = Instantiate(entity_prefab);
		entity_obj.transform.parent = GameObject.Find("/Game/Entities").transform;
		entity_obj.transform.localPosition = transformLocation(entity["location"]);
		entity_obj.name = entity["ID"].intValue.ToString();
		return entity_obj;
	}

	public void UpdateObjectLocations(List<GameObject> entities, List<Vector3> locations) {
		for (int i = 0; i < entities.Count; i++) {
			GameObject entity = entities[i];
			Vector3 location = locations[i];
			if (entity.GetComponent<SpriteRenderer>().isVisible) {
				iTween.RotateTo(entity, iTween.Hash("rotation", new Vector3(0, 0, Mathf.Atan2(location.y - entity.transform.localPosition.y, location.x - entity.transform.localPosition.x) * Mathf.Rad2Deg - 90), "time", 0.1f));
				iTween.MoveTo(entity, location, 1f / Mathf.Max(playSpeed, 0.1f));
			} else {
				entity.transform.localPosition = location;
			}
		}
	}

	public void onProgressChanged() {
		if ((int)progressBar.mainSlider.value != currentRound && demo != null) {
			currentRound = (int)progressBar.mainSlider.value;
			runRound(demo["rounds"].list[currentRound]);
		}
	}

	public void onSpeedChanged() {
		playSpeed = speedController.currentValue;
	}

	public void runRound(JSONObject round) {
		progressBar.mainSlider.value = currentRound;
		List<GameObject> updateEntities = new List<GameObject>();
		List<Vector3> updateLocations = new List<Vector3>();
		HashSet<int> entityIds = new HashSet<int>();
		foreach (JSONObject entity in round) {
			entityIds.Add(entity["ID"].intValue);
			GameObject entity_obj = GameObject.Find("/Game/Entities/" + entity["ID"].intValue.ToString());
			if (entity_obj == null) {
				entity_obj = addEntity(entity);
			}
			entity_obj.GetComponent<EntityController>().update(entity);
			if (entity_obj.transform.localPosition != transformLocation(entity["location"])) {
				updateEntities.Add(entity_obj);
				updateLocations.Add(transformLocation(entity["location"]));
			}
		}
		foreach (GameObject entity_obj in GameObject.FindGameObjectsWithTag("Entity")) {
			if (!entityIds.Contains(entity_obj.GetComponent<EntityController>().ID)) {
				Destroy(entity_obj);
			}
		}
		UpdateObjectLocations(updateEntities, updateLocations);
	}

	IEnumerator playDemo() {
		progressBar.mainSlider.maxValue = demo["rounds"].list.Count - 1;
		while (true) {
			if (paused) {
				yield return null;
				continue;
			}
			currentRound = Math.Clamp(currentRound, 0, demo["rounds"].list.Count - 1);
			JSONObject round = demo["rounds"].list[currentRound];
			runRound(round);
			currentRound++;
			yield return new WaitForSeconds(0.1f / Mathf.Max(playSpeed, 0.1f));
		}
	}

	void switchPause() {
		paused = !paused;
		demoPanel.SetActive(paused);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			switchPause();
		}
	}

	void Start() {
		getAllDemoNames();
		foreach (string demoName in demoNames) {
			// Debug.Log(demoName);
			demoList.CreateNewItem(demoName);
		}
		demoList.SetupDropdown();
		demoList.onValueChanged.AddListener(updateDemo);
		demoList.Animate();
	}

	void updateDemo(int index) {
		if (isPlaying) {
			StopAllCoroutines();
			deleteMap();
			isPlaying = false;
		}
		readDemo(demoNames[index]);
		prepareMap();
		currentRound = 0;
		StartCoroutine(playDemo());
		isPlaying = true;
		switchPause();
	}
}
