using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Defective.JSON;

public class DemoManager : MonoBehaviour
{
	public float playSpeed = 1.0f;

	private JSONObject demo;
	private int currentRound;

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

	public void prepareMap() {
		GameObject map_prefab = Resources.Load("Prefabs/map") as GameObject;
		GameObject map = Instantiate(map_prefab);
		map.name = "Map";
		map.transform.localScale = new Vector3(demo["map"]["size"][0].floatValue, demo["map"]["size"][1].floatValue, 0);
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
			// Debug.Log("Moving entity " + entity.name + " from " + entity.transform.localPosition + " to " + location);
			if (entity.GetComponent<SpriteRenderer>().isVisible) {
				iTween.RotateTo(entity, iTween.Hash("rotation", new Vector3(0, 0, Mathf.Atan2(location.y - entity.transform.localPosition.y, location.x - entity.transform.localPosition.x) * Mathf.Rad2Deg - 90), "time", 0.1f));
				iTween.MoveTo(entity, location, 1f / playSpeed);
			} else {
				entity.transform.localPosition = location;
			}
		}
	}

	public void runRound(JSONObject round) {
		List <GameObject> updateEntities = new List<GameObject>();
		List <Vector3> updateLocations = new List<Vector3>();
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
		foreach (JSONObject round in demo["rounds"]) {
			runRound(round);
			yield return new WaitForSeconds(0.1f / playSpeed);
		}
	}

	void Start() {
		readDemo("replays-debug");
		prepareMap();
		StartCoroutine(playDemo());
	}
}
