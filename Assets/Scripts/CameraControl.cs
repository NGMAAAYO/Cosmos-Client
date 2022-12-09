using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class CameraControl : MonoBehaviour
{

    private Vector3 Origin;
    private Vector3 Difference;
    private Vector3 ResetCamera;
    private bool drag = false;
    private float horizontalBound;
    private float verticalBound;
    public float scroll_sensitivity = 0.5f;
    public float scroll_limit_upper = 40f;
    public float scroll_limit_lower = 2f;
    private float camera_ratio;


    // Start is called before the first frame update
    void Start()
    {
        verticalBound = GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite.texture.height/25;
        horizontalBound = GameObject.Find("Background").GetComponent<SpriteRenderer>().sprite.texture.width/25;
        ResetCamera = Camera.main.transform.position;
        camera_ratio = Camera.main.aspect;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButton(0)) {
            Difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (drag == false) {
                drag = true;
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else {
            drag = false;
        }
        if  (drag) {
            Camera.main.transform.position = Origin - Difference;
        }
        if (Input.GetMouseButton(1)) {
            Camera.main.transform.position = ResetCamera;
        }
        // setup and clamp the range of mouse scroll
        float delta_size = Input.mouseScrollDelta.y * scroll_sensitivity;
        Camera.main.orthographicSize -= delta_size;

        float cam_orthsize = Mathf.Clamp(Camera.main.orthographicSize, scroll_limit_lower, scroll_limit_upper);
        Camera.main.orthographicSize = cam_orthsize;

        float range_x = Mathf.Clamp(Camera.main.transform.position.x, -horizontalBound+Camera.main.orthographicSize*camera_ratio, horizontalBound-Camera.main.orthographicSize*camera_ratio);
        float range_y = Mathf.Clamp(Camera.main.transform.position.y, -verticalBound+Camera.main.orthographicSize, verticalBound-Camera.main.orthographicSize);
        if (horizontalBound <= Camera.main.orthographicSize*camera_ratio || verticalBound <= Camera.main.orthographicSize) {
            Debug.Log("11111111111111111");
            range_x = 0; 
            scroll_limit_upper = Mathf.Clamp(scroll_limit_upper, scroll_limit_lower, horizontalBound/camera_ratio);
        } // still having logic error


        Camera.main.transform.position = new Vector3(range_x, range_y, Camera.main.transform.position.z);

        

    }
}
