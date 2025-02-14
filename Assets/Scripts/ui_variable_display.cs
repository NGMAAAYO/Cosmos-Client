using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ui_variable_display : MonoBehaviour
{
	public List<TMP_Text> TextComponents;
	public GameObject ui_panel;
	public GameObject entity_indicator;
	private GameObject entity_obj;
	private int entityID;

	void Start() 
	{
		ui_panel.SetActive(false);
		entity_indicator.SetActive(false);
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			Ray myRay = new Ray(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y),Vector2.zero);
			var hitP = Physics2D.RaycastAll(myRay.origin,myRay.direction);
			foreach(RaycastHit2D elem in hitP){
				if (elem.collider.gameObject.tag == "Entity") {
					entityID = elem.collider.gameObject.GetComponent<EntityController>().ID;
					ui_panel.SetActive(true);
					entity_indicator.SetActive(true);
				}
			}
		}
		if (Input.GetKey(KeyCode.Escape)) {
			entityID = 0;
			ui_panel.SetActive(false);
			entity_indicator.SetActive(false);
		}

		if (entityID != 0) {
			entity_obj = GameObject.Find("/Game/Entities/" + entityID.ToString());
			if (entity_obj == null) {
				entityID = 0;
				entity_indicator.SetActive(false);
				return;
			}
			EntityController ec = entity_obj.GetComponent<EntityController>();
			TextComponents[0].text = "ID: " + ec.ID;
			TextComponents[1].text = "Energy: " + ec.energy;
			TextComponents[2].text = "Defence: " + ec.defence;
			TextComponents[3].text = "Team: " + ec.team;
			TextComponents[4].text = "Type: " + ec.type;
			TextComponents[5].text = "Radio: " + ec.radio;
			if (ec.type == "destroyer") {
				entity_indicator.GetComponent<IndicatorRenderer>().circleRadius = 25f;
			} else if (ec.type == "miner") {
				entity_indicator.GetComponent<IndicatorRenderer>().circleRadius = 20f;
			} else if (ec.type == "scout") {
				entity_indicator.GetComponent<IndicatorRenderer>().circleRadius = 30f;
			} else {
				entity_indicator.GetComponent<IndicatorRenderer>().circleRadius = 40f;
			}
			entity_indicator.GetComponent<IndicatorRenderer>().location = entity_obj.transform.position;
		}
	}
}
