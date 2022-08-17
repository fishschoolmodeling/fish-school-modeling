using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObstacle : MonoBehaviour
{
    private bool isClickedOn = false;

    void Update()
    {
        if (isClickedOn) {
            GameObject.Find("Main Camera").GetComponent<CameraControl>().isPickingUpObjects = true;
            Vector3 cursorToScenePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(cursorToScenePos.x, cursorToScenePos.y, transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(gameObject);
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) {
            isClickedOn = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isClickedOn = false;
            GameObject.Find("Main Camera").GetComponent<CameraControl>().isPickingUpObjects = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(gameObject);
        }
    }
}
