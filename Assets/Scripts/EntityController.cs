using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defective.JSON;

public class EntityController : MonoBehaviour
{
	public int ID;
	public float energy;
	public float defence;
	public string team;
	public string type;
	public int radio;
	public List <Color> teamColors;
	public List <Sprite> entityIcons;

	public void change_color(string t) {
		if (t == "Neutral") {
			GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.4f, 0.4f);
		} else {
			GetComponent<SpriteRenderer>().color = teamColors[int.Parse(t)];
		}
	}

	public void change_type(string t) {
		if (t == "planet") {
			GetComponent<SpriteRenderer>().sprite = entityIcons[0];
		} else if (t == "destroyer") {
			GetComponent<SpriteRenderer>().sprite = entityIcons[1];
		} else if (t == "miner") {
			GetComponent<SpriteRenderer>().sprite = entityIcons[2];
		} else {
			GetComponent<SpriteRenderer>().sprite = entityIcons[3];
		}
	}

	public void update(JSONObject entity) {
		ID = entity["ID"].intValue;
		energy = entity["energy"].floatValue;
		defence = entity["defence"].floatValue;
		radio = entity["radio"].intValue;
		if (type != entity["type"].stringValue) {
			type = entity["type"].stringValue;
			change_type(type);
		}
		if (team != entity["team"].stringValue) {
			team = entity["team"].stringValue;
			change_color(team);
		}
	}
}
