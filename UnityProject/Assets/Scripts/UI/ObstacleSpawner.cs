using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject prefab;

    private void OnMouseDown()
    {
        GameObject createdObstacle = Instantiate(prefab);
        // Make sure created obstacles are interactable, and not hidden behind other objects (by moving it to z=-73.)
        createdObstacle.transform.position = new Vector3(createdObstacle.transform.position.x, createdObstacle.transform.position.y, -73);
    }
}
