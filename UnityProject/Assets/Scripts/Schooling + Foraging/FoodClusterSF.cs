using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodClusterSF : MonoBehaviour {
    public GameObject food;
    public bool respawnFood;
    public float width;
    public float height;
    [field: SerializeField, ReadOnlyField]
    public int maxFoodAmount = 200;
    [field: SerializeField, ReadOnlyField]
    public int totalFoodAmount;
    public int foodSpawnCap = 10;
    private List<GameObject> foodArray = new List<GameObject>();
    public FishTankSF myTank;
    // Start is called before the first frame update
    void Start() {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
        this.totalFoodAmount = this.maxFoodAmount;
        CreateFood(foodSpawnCap, food);
    }

    // Update is called once per frame
    void Update() {
    }

    void FixedUpdate() {
        bool empty = true;
        for (int i = 0; i < foodArray.Count; i++) {
            if (foodArray[i] != null) empty = false;
        }
        if (empty || totalFoodAmount <= 0) {
            if (!empty) {
                for (int i = 0; i < foodArray.Count; i++) {
                    if (foodArray[i] != null) Destroy(foodArray[i]);
                }
            }
            Transform wall = myTank.transform.Find("Wall");
            Transform upperBorder = wall.Find("borderU");
            Transform leftBorder = wall.Find("borderL");
            float widthRange = transform.lossyScale.x - (upperBorder.lossyScale.x);
            float heightRange = transform.lossyScale.y - (leftBorder.lossyScale.y);
            transform.position = new Vector3(Random.Range(-widthRange / 2, widthRange / 2), Random.Range(-heightRange / 2, heightRange / 2),
                    0f) + myTank.transform.position;
            CreateFood(foodSpawnCap, food);
            totalFoodAmount = maxFoodAmount;
        }
    }

    void CreateFood(int num, GameObject type) {
        for (int i = 0; i < num; i++) {
            width = GetComponent<SpriteRenderer>().bounds.size.x;
            height = GetComponent<SpriteRenderer>().bounds.size.y;
            GameObject food = Instantiate(type, new Vector3(Random.Range(-width / 2, width / 2), Random.Range(-height / 2, height / 2),
                    0f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            food.GetComponent<FoodLogicSF>().respawn = respawnFood;
            food.GetComponent<FoodLogicSF>().myCluster = this;
            foodArray.Add(food);
        }
    }
}
