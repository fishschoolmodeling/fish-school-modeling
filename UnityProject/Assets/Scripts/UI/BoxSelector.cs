using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSelector : MonoBehaviour
{
    public Collider2D[] selectedColliders;
    private LineRenderer lineRender;
    public Vector2 originMousePos;
    public Vector2 currentMousePos;

    public Sprite fishSpr;
    public Sprite fishHighlightSpr;

    // Start is called before the first frame update
    void Start()
    {
        lineRender = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            foreach (Collider2D selection in selectedColliders)
            {
                selection.gameObject.GetComponent<SpriteRenderer>().sprite = fishSpr;
            }

            selectedColliders = Physics2D.OverlapAreaAll(new Vector2(-10000, -10000), new Vector2(-10000, -10000), LayerMask.GetMask("Agent"));

            if (GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho == gameObject)
            {
                GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho = null;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            originMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            lineRender.positionCount = 4;
            lineRender.SetPosition(0, new Vector2(originMousePos.x, originMousePos.y));
            lineRender.SetPosition(1, new Vector2(originMousePos.x, currentMousePos.y));
            lineRender.SetPosition(2, new Vector2(currentMousePos.x, currentMousePos.y));
            lineRender.SetPosition(3, new Vector2(currentMousePos.x, originMousePos.y));

            selectedColliders = Physics2D.OverlapAreaAll(originMousePos, currentMousePos, LayerMask.GetMask("Agent"));
            //print(originMousePos + "; " + currentMousePos + "; # of Fish selected: " + selectedColliders.Length);
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (selectedColliders.Length > 0)
            {
                GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho = gameObject;
                GameObject.Find("Main Camera").GetComponent<CameraControl>().followName = "Group of " + selectedColliders.Length + " fish";
                GameObject.Find("Main Camera").GetComponent<CameraControl>().framesFollowed = 0;
            }

            lineRender.positionCount = 0;
            Physics2D.OverlapAreaAll(new Vector2(-10000, -10000), new Vector2(-10000, -10000), LayerMask.GetMask("Agent"));
        }

        followSelection();
    }

    void followSelection() {
        int selectionCount = selectedColliders.Length;
        float sumX = 0;
        float sumY = 0;

        if (selectionCount > 0)
        {
            foreach (Collider2D selection in selectedColliders)
            {
                selection.gameObject.GetComponent<SpriteRenderer>().sprite = fishHighlightSpr;
                sumX += selection.gameObject.transform.position.x;
                sumY += selection.gameObject.transform.position.y;
            }

            transform.position = new Vector3(sumX / selectionCount, sumY / selectionCount, transform.position.z);
        }
    }
}
