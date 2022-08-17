using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using System;

public class FishTankSF : MonoBehaviour {
    public GameObject predatorPrefab;
    public GameObject clusterPrefab;
    public GameObject fishPrefab;
    public GameObject blockPrefab;
    [field: SerializeField, ReadOnlyField]
    private int numPredators;
    [field: SerializeField, ReadOnlyField]
    private int numFish;
    [field: SerializeField, ReadOnlyField]
    private int numCluster;
    [field: SerializeField, ReadOnlyField]
    private float fishSpawnRange;
    public List<FoodClusterSF> foodClusters = new List<FoodClusterSF>();
    public List<FishSFAgent> fishes = new List<FishSFAgent>();

    public List<Predator> predators = new List<Predator>();

    public Block[,] gridBlocks;

    public List<List<FishSFAgent>> fishGroups = new List<List<FishSFAgent>>();
    float tankWidth = 0f;
    float tankHeight = 0f;

    FishTrainer m_FishTrainer;

    private float nextUpdate = 0.5f;

    private void Start() {
        m_FishTrainer = FindObjectOfType<FishTrainer>();

        this.numPredators = m_FishTrainer.predatorsPerTank;
        this.numFish = m_FishTrainer.fishPerTank;
        this.numCluster = m_FishTrainer.clustersPerTank;
        this.fishSpawnRange = m_FishTrainer.fishSpawnRange;

        for (int i = 0; i < numFish; i++) {
            GameObject fishGameObject = Instantiate(fishPrefab, new Vector3(0f, 0f, 0f) + transform.position,
                  Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            fishGameObject.transform.parent = this.transform;
            FishSFAgent fishComponent = fishGameObject.GetComponent<FishSFAgent>();
            fishComponent.tank = this;
            fishes.Add(fishComponent);
        }
        for (int i = 0; i < numPredators; i++) {
            GameObject predator = Instantiate(predatorPrefab, new Vector3(0f, 0f, 0f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            predator.transform.parent = this.transform;
            Predator predatorComponent = predator.GetComponent<Predator>();
            predatorComponent.tank = this;
            predators.Add(predatorComponent);
            ResetAgent(predator.transform);
        }
    }

    private List<List<FishSFAgent>> GetFishGroups() {
        List<List<FishSFAgent>> groups = new List<List<FishSFAgent>>();
        List<FishSFAgent> checkedFishes = new List<FishSFAgent>();
        foreach (FishSFAgent fish in fishes) {
            if (!checkedFishes.Contains(fish)) {
                List<FishSFAgent> currentNetwork = new List<FishSFAgent>();
                currentNetwork.Add(fish);
                findNetwork(fish, checkedFishes, currentNetwork);
                groups.Add(currentNetwork);
            }
        }
        return groups;
    }

    private void findNetwork(FishSFAgent currentFish, List<FishSFAgent> checkedFishes, List<FishSFAgent> currentNetwork) {
        checkedFishes.Add(currentFish);
        foreach (NeighborFish neighborFish in currentFish.neighborFishes) {
            if (!checkedFishes.Contains(neighborFish.FishComponent)) {
                currentNetwork.Add(neighborFish.FishComponent);
                findNetwork(neighborFish.FishComponent, checkedFishes, currentNetwork);
            }
        }
    }

    private void FixedUpdate() {
        foreach (FishSFAgent fish in fishes) {
            Vector3 shiftVector = new Vector3(-tankWidth * 0.5f, tankHeight * 0.5f, 0);
            Vector2 origin = transform.position + shiftVector;
            Vector2 fishPosition = fish.transform.position;
            Vector2 offset = fishPosition - origin;
            int gridPosX = (int)Math.Floor(offset.x / 40f);
            int gridPosY = (int)Math.Floor(-offset.y / 40f);

            Block currentBlock = gridBlocks[gridPosX, gridPosY];
            if (fish.block) {
                if (fish.block != currentBlock) {
                    fish.block.fishInBlock.Remove(fish);
                    fish.block = currentBlock;
                    currentBlock.fishInBlock.Add(fish);
                }
            } else {
                fish.block = currentBlock;
                currentBlock.fishInBlock.Add(fish);
            }
        }

        foreach (Predator predator in predators) {
            Vector3 shiftVector = new Vector3(-tankWidth * 0.5f, tankHeight * 0.5f, 0);
            Vector2 origin = transform.position + shiftVector;
            Vector2 predatorPosition = predator.transform.position;
            Vector2 offset = predatorPosition - origin;
            int gridPosX = (int)Math.Floor(offset.x / 40f);
            int gridPosY = (int)Math.Floor(-offset.y / 40f);
            Block currentBlock = gridBlocks[gridPosX, gridPosY];
            if (predator.block) {
                if (predator.block != currentBlock) {
                    predator.block.predatorsInBlock.Remove(predator);
                    predator.block = currentBlock;
                    currentBlock.predatorsInBlock.Add(predator);
                }
            } else {
                predator.block = currentBlock;
                currentBlock.predatorsInBlock.Add(predator);
            }
        }

        for (int i = 0; i < gridBlocks.GetLength(0); i++) {
            for (int j = 0; j < gridBlocks.GetLength(1); j++) {
                Block block = gridBlocks[i, j].GetComponent<Block>();
                if (block.fishInBlock.Count > 0) {
                    // block.spriteRenderer.color = new Color32(255, 255, 0, 25);
                } else {
                    block.spriteRenderer.color = new Color32(11, 83, 253, 25);
                }
            }
        }
    }

    private void Update() {
        if (Time.time >= nextUpdate) {
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            UpdateOnSchedule();
        }
    }

    private void UpdateOnSchedule() {
        fishGroups = GetFishGroups();
        List<int> groupings = new List<int>();
        foreach (List<FishSFAgent> group in fishGroups) {
            groupings.Add(group.Count);
        }
        m_FishTrainer.updateFishGrouping(groupings);
    }

    void CreateFoodCluster(int num, GameObject clusterObject, float cluster_level) {
        Transform wall = transform.Find("Wall");
        Transform upperBorder = wall.Find("borderU");
        Transform leftBorder = wall.Find("borderL");
        float widthRange = clusterObject.transform.lossyScale.x - (upperBorder.lossyScale.x);
        float heightRange = clusterObject.transform.lossyScale.y - (leftBorder.lossyScale.y);
        for (int i = 0; i < num; i++) {
            GameObject cluster = Instantiate(clusterObject, new Vector3(0f, 0f, 0f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            FoodClusterSF clusterComponent = cluster.GetComponent<FoodClusterSF>();
            clusterComponent.maxFoodAmount = m_FishTrainer.maxFoodAmount;
            clusterComponent.respawnFood = true;
            clusterComponent.myTank = this;
            float x_scale = cluster.transform.localScale.x * cluster_level;
            float y_scale = cluster.transform.localScale.y * cluster_level;
            cluster.transform.localScale = new Vector3(x_scale, y_scale, 1);
            foodClusters.Add(clusterComponent);
        }
    }

    public void ResetTank(GameObject[] agents, float cluster_level) {
        Transform wall = transform.Find("Wall");
        wall.localScale = new Vector3(1 / cluster_level, 1 / cluster_level, 1);
        tankWidth = wall.Find("borderU").transform.localScale.x * wall.localScale.x;
        tankHeight = wall.Find("borderR").transform.localScale.y * wall.localScale.x;
        int numHorizontalGrid = (int)(tankWidth / blockPrefab.transform.localScale.x);
        int numVerticalGrid = (int)(tankHeight / blockPrefab.transform.localScale.y);
        if (cluster_level >= 0.4)
            CreateFoodCluster(numCluster, clusterPrefab, cluster_level);
        else
            CreateFoodCluster(3, clusterPrefab, cluster_level);
        CreateBlocks(numHorizontalGrid, numVerticalGrid, tankWidth, tankHeight);

        foreach (GameObject agent in agents) {
            if (agent.transform.parent == gameObject.transform) {
                ResetAgent(agent.transform);
            }
        }
    }

    void CreateBlocks(int numHorizontalGrid, int numVerticalGrid, float tankWidth, float tankHeight) {
        gridBlocks = new Block[numHorizontalGrid + 1, numVerticalGrid + 1];
        for (int i = 0; i < numHorizontalGrid + 1; i++) {
            for (int j = 0; j < numVerticalGrid + 1; j++) {
                GameObject block = Instantiate(blockPrefab, new Vector3(-tankWidth * 0.5f + 40 * i, tankHeight * 0.5f - 40 * j, 0f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
                block.transform.parent = this.transform;
                Block blockComponent = block.GetComponent<Block>();
                blockComponent.spriteRenderer = blockComponent.transform.Find("BlockSprite").GetComponent<SpriteRenderer>();
                gridBlocks[i, j] = blockComponent;
                blockComponent.blockXPos = i;
                blockComponent.blockYPos = j;
            }
        }
    }

    public void ResetAgent(Transform agent) {
        agent.position = new Vector3(UnityEngine.Random.Range(-fishSpawnRange, fishSpawnRange), UnityEngine.Random.Range(-fishSpawnRange, fishSpawnRange),
                    0f)
                    + transform.position;
        agent.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0, 360)));
    }
}
