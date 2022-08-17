using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    private Vector3 ResetCamera; // original camera position
    private Vector3 Origin; // place where mouse is first pressed
    private Vector3 Difference; // change in position of mouse relative to origin

    public GameObject followWho;

    private float ResetZoom;
    public float framesFollowed = 0;

    public Text uiText;
    public string followName = "";
    public bool removeCloneFromFollowName = true;

    public Text controlsText;
    public Text controlsText2;

    public bool isPickingUpObjects = false;

    // Start is called before the first frame update
    void Start()
    {
        ResetCamera = Camera.main.transform.position;
        ResetZoom = Camera.main.orthographicSize;

        if (uiText == null) uiText = GameObject.Find("UI Text").GetComponent<Text>();
        if (controlsText == null) controlsText = GameObject.Find("Controls Text").GetComponent<Text>();
        if (controlsText2 == null) controlsText = GameObject.Find("Controls Text 2").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (followWho != null)
        {
            transform.position = new Vector3(followWho.transform.position.x, followWho.transform.position.y, transform.position.z);
            framesFollowed += 1;
        }

        if (Input.GetMouseButtonDown(0) && !isPickingUpObjects)
        {
            Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (framesFollowed > 1)
            {
                followWho = null;
                followName = "";
            }
        }

        if (Input.GetMouseButton(0) && !isPickingUpObjects)
        {
            Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            transform.position = Origin - Difference;
        }

        if (Input.GetKeyDown(KeyCode.R)) // Reset camera to original position
        {
            transform.position = ResetCamera;
            Camera.main.orthographicSize = ResetZoom;
            followWho = null;
        }

        Camera.main.orthographicSize -= 8 * Input.mouseScrollDelta.y;
        if (Camera.main.orthographicSize < 8) Camera.main.orthographicSize = 8;

        //if (Input.GetMouseButton(1)) followWho = null;

        // Game Speed Controls
        if (Input.GetKeyDown(KeyCode.A) && Time.timeScale > 0f)
        {
            Time.timeScale += -0.25f;
            Time.timeScale = Mathf.Round(100 * Time.timeScale) / 100;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Time.timeScale += 0.25f;
            Time.timeScale = Mathf.Round(100 * Time.timeScale) / 100;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
            else
            {
                Time.timeScale = 0f;
            }
            
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            controlsText.enabled = controlsText.enabled ? false : true;
            controlsText2.enabled = controlsText2.enabled ? false : true;
        }

        uiText.text = "Camera Position: (" + Mathf.Round(100 * transform.position.x)/100 + ", " + Mathf.Round(100 * transform.position.y)/100 + ")"
            + "\nCamera Zoom : " + Mathf.Round(100 * ResetZoom / Camera.main.orthographicSize) / 100
            + "x\nSimulation Speed: " + Time.timeScale + "x";

        if (followName != "") {
            if (removeCloneFromFollowName)
            {
                uiText.text += "\nFollowing: " + followName.Replace("(Clone)", "");
            }
            else
            { 
                uiText.text += "\nFollowing: " + followName;
            }
        }

        //uiText.text += "\n\n[Space] Reset Camera";
    }
}