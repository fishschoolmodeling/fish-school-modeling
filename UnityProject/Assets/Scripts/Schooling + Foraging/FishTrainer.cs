using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using System.Collections.Generic;
using System.Linq;
using System;

public class FishTrainer : MonoBehaviour {
    [Serializable]
    public class Statistics {
        public Statistics(
            int steps,
            float avgNeighbors,
            int foodEaten,
            float totalScore,
            int totalAgentHitCount,
            int totalWallHitCount,
            int fishEaten,
            float totalFish,
            float avgFramesNearWall,
            float agentSatiatedRatio,
            float globalPolarization,
            float meanSpeed,
            float meanFoodIntensity,
            float meanPredatorIntensity,
            float meanDirection
         ) {
            this.steps = steps;
            this.foodEaten = foodEaten;
            this.totalScore = totalScore;
            this.totalAgentHitCount = totalAgentHitCount;
            this.totalWallHitCount = totalWallHitCount;
            this.fishEaten = fishEaten;
            this.totalFish = totalFish;
            this.avgFramesNearWall = avgFramesNearWall;
            this.agentSatiatedRatio = agentSatiatedRatio;
            this.globalPolarization = globalPolarization;
            this.meanSpeed = meanSpeed;
            this.meanFoodIntensity = meanFoodIntensity;
            this.meanPredatorIntensity = meanPredatorIntensity;
            this.meanDirection = meanDirection;
            this.avgNeighbors = avgNeighbors;
        }
        public int steps;
        public int foodEaten;
        public float totalScore;
        public int totalAgentHitCount;
        public int totalWallHitCount;
        public int fishEaten;
        public float totalFish;
        public float avgFramesNearWall;
        public float agentSatiatedRatio;
        public float globalPolarization;
        public float meanSpeed;
        public float meanFoodIntensity;
        public float meanPredatorIntensity;
        public float meanDirection;
        public float avgNeighbors;
    }
    List<Statistics> statistics = new List<Statistics>();
    [Header("Max Simulation Steps")]
    [field: SerializeField]
    public long maxSimulationSteps = 10000000000000;
    [Header("Statistics")]
    [field: SerializeField, ReadOnlyField]
    private float avgNeighbors = 0f;
    [field: SerializeField, ReadOnlyField]
    public int foodEaten = 0;
    [field: SerializeField, ReadOnlyField]
    public float totalScore = 0;
    [field: SerializeField, ReadOnlyField]
    public int totalAgentHitCount = 0;
    [field: SerializeField, ReadOnlyField]
    public int totalWallHitCount = 0;
    [field: SerializeField, ReadOnlyField]
    public int fishEaten = 0;
    [field: SerializeField, ReadOnlyField]
    private float totalFish = 1;
    [field: SerializeField, ReadOnlyField]
    public float avgFramesNearWall = 0;
    [field: SerializeField, ReadOnlyField]
    public int totalFramesNearWall = 0;
    [field: SerializeField, ReadOnlyField]
    public float agentSatiatedRatio = 0;
    [field: SerializeField, ReadOnlyField]
    public float globalPolarization = 0;
    [field: SerializeField, ReadOnlyField]
    public Vector2 meanDirection = new Vector2(0, 0);
    [field: SerializeField, ReadOnlyField]
    public float meanSpeed = 0;
    [field: SerializeField, ReadOnlyField]
    public float meanFoodIntensity = 0;
    [field: SerializeField, ReadOnlyField]
    public float meanPredatorIntensity = 0;
    [Header("Tank Settings")]
    public int fishPerTank;
    public int predatorsPerTank;
    public int clustersPerTank;
    public int fishSpawnRange;
    public int maxFoodAmount;
    [Header("Agent Settings")]
    // Abilities
    [field: SerializeField]
    public float agentSteerStrength = 25f;
    [field: SerializeField]
    public float agentMaxSpeed = 20f;
    [field: SerializeField]
    public float agentMinSpeed = 10f;
    [field: SerializeField]
    public float agentAccelerationConstant = 50f;
    [field: SerializeField]
    public float agentNeighborSensorRadius = 40f;
    [field: SerializeField]
    public float agentPredatorSensorRadius = 65f;
    [field: SerializeField]
    public int satiateDuration = 10;
    [field: SerializeField]
    public int satiateDecay = 10;

    private int simulationSteps = 0;
    private int statRecordTimer = 0;


    [System.Serializable]
    public class Rewards {
        public Rewards(float onEatenReward, float eatReward, float loseNeighborReward, float wallCrashReward, float neighborCrashReward, float idleReward, float predatorDistanceReward, float seeNeighborReward) {
            this.onEatenReward = onEatenReward;
            this.eatReward = eatReward;
            this.loseNeighborReward = loseNeighborReward;
            this.wallCrashReward = wallCrashReward;
            this.neighborCrashReward = neighborCrashReward;
            this.idleReward = idleReward;
            this.predatorDistanceReward = predatorDistanceReward;
            this.seeNeighborReward = seeNeighborReward;
        }
        public float onEatenReward, eatReward, loseNeighborReward, wallCrashReward, neighborCrashReward, idleReward, predatorDistanceReward, seeNeighborReward;
    }
    // Rewards
    [Header("Rewards")]
    [field: SerializeField]
    public Rewards rewards = new Rewards(0, 0, 0, 0, 0, 0, 0, 0);
    // [field: SerializeField]
    // public float onEatenReward = -100f;
    // [field: SerializeField]
    // public float eatReward = 2.5f;
    // [field: SerializeField]
    // public float loseNeighborReward = -0.3f;
    // [field: SerializeField]
    // public float wallCrashReward = -3f;
    // [field: SerializeField]
    // public float neighborCrashReward = -3f;
    // [field: SerializeField]
    // public float idleReward = 0f;
    // [field: SerializeField]
    // public float predatorDistanceReward = -1f;
    // [field: SerializeField]
    // public float agentSeeNeighborReward = 0.01f;

    [Header("Predator Settings")]
    [field: SerializeField]
    public float predatorCruiseSpeed = 10f;
    [field: SerializeField]
    public float predatorChaseSpeed = 22f;
    [field: SerializeField]
    public float predatorVisibleRadius = 40f;
    [field: SerializeField]
    public float predatorSteerStrength = 5f;
    [field: SerializeField]
    public float predatorChaseForceMagnitude = 30f;
    [field: SerializeField]
    public int predatorMaxStomach = 10;
    [field: SerializeField]
    public bool predatorSwimToCluster = false;
    [Header("UI Settings")]
    public Text scoreText;
    public bool renderVoronoiSelected = true;
    public bool renderVisionConeSelected = false;
    public bool renderNeighborSensorSelected = false;
    public bool renderNeighborRaySelected = true;
    public bool renderPredatorSensorSelected = false;
    public bool renderPredatorRaySelected = true;
    public bool renderNeighborRayAll = false;
    [Header("Trainer Settings")]
    [field: SerializeField]
    public float defaultClusterLevel = 1f;
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FishTankSF[] listArea;
    [HideInInspector]
    private int avgNeighborTicker = 0;
    private List<int> fishGroups = new List<int>();
    private int run_count = -1;


    StatsRecorder m_Recorder;
    EnvironmentParameters m_ResetParams;

    private float cluster_level;

    public void Awake() {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_Recorder = Academy.Instance.StatsRecorder;
        int current_run_count = 0;
        while (System.IO.File.Exists(Application.dataPath + $"/simulation_runs/run_{current_run_count}.json")) {
            current_run_count += 1;
        }
        this.run_count = current_run_count;
    }

    void EnvironmentReset() {
        cluster_level = m_ResetParams.GetWithDefault("food_cluster", defaultClusterLevel);
        float onEatenReward = m_ResetParams.GetWithDefault("on_eaten_reward", 0);
        float eatReward = m_ResetParams.GetWithDefault("eat_reward", 0);
        float loseNeighborReward = m_ResetParams.GetWithDefault("lose_neighbor_reward", 0);
        float wallCrashReward = m_ResetParams.GetWithDefault("wall_crash_reward", 0);
        float neighborCrashReward = m_ResetParams.GetWithDefault("neighbor_crash_reward", 0);
        float idleReward = m_ResetParams.GetWithDefault("idle_reward", 0);
        float predatorDistanceReward = m_ResetParams.GetWithDefault("predator_distance_reward", 0);
        float seeNeighborReward = m_ResetParams.GetWithDefault("see_neighbor_reward", 0);

        this.rewards.onEatenReward = onEatenReward;
        this.rewards.eatReward = eatReward;
        this.rewards.loseNeighborReward = loseNeighborReward;
        this.rewards.wallCrashReward = wallCrashReward;
        this.rewards.neighborCrashReward = neighborCrashReward;
        this.rewards.idleReward = idleReward;
        this.rewards.predatorDistanceReward = predatorDistanceReward;
        this.rewards.seeNeighborReward = seeNeighborReward;

        Debug.Log("Scene initialized with the following settings: \n" +
        $"cluster level: {cluster_level} \n" +
        $"on_eaten_reward: {rewards.onEatenReward} \n" +
        $"eat_reward: {rewards.eatReward} \n" +
        $"lose_neighbor_reward: {rewards.loseNeighborReward} \n" +
        $"wall_crash_reward: {rewards.wallCrashReward} \n" +
        $"neighbor_crash_reward: {rewards.neighborCrashReward} \n" +
        $"idle_reward: {rewards.idleReward} \n" +
        $"predator_distance_reward: {rewards.predatorDistanceReward} \n" +
        $"see_neighbor_reward: {rewards.seeNeighborReward}"
        );

        ClearObjects(GameObject.FindGameObjectsWithTag("food"));
        ClearObjects(GameObject.FindGameObjectsWithTag("food_cluster"));
        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<FishTankSF>();
        FoodClusterSF[] listCluster = FindObjectsOfType<FoodClusterSF>();
        Block[] listBlock = FindObjectsOfType<Block>();
        foreach (var cluster in listCluster) {
            Destroy(cluster.gameObject);
        }
        foreach (var block in listBlock) {
            Destroy(block.gameObject);
        }
        foreach (var fa in listArea) {
            fa.ResetTank(agents, cluster_level);
        }
        int agentCount = agents.Count();
        totalFish = agentCount > 0f ? (float)agentCount : 1f;
        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects) {
        foreach (var food in objects) {
            Destroy(food);
        }
    }

    public void updateFishGrouping(List<int> newGroupings) {
        fishGroups = newGroupings;
    }

    public void UpdateNeighborCount(int count) {
        avgNeighbors = (avgNeighbors * avgNeighborTicker + count) / (avgNeighborTicker + 1);
        avgNeighborTicker += 1;
    }

    public void UpdateFramesNearWall() {
        totalFramesNearWall++;
        avgFramesNearWall = totalFramesNearWall / totalFish;
    }

    public void UpdateAgentsSatiatedRatio() {
        int tempCount = 0;
        foreach (GameObject agentObject in agents) {
            FishSFAgent agent = agentObject.GetComponent<FishSFAgent>();
            if (agent.stomach > 0) tempCount++;
        }
        this.agentSatiatedRatio = (float)tempCount / totalFish;
    }

    private void updateGlobalProperties() {
        float sumSpeed = 0;
        Vector2 sumDirection = new Vector2(0, 0);
        float sumPolarization = 0;
        float sumFoodIntensity = 0;
        float sumPredatorIntensity = 0;
        foreach (GameObject agentObject in agents) {
            FishSFAgent agent = agentObject.GetComponent<FishSFAgent>();
            sumSpeed += agent.rb.velocity.magnitude;
            sumDirection += agent.rb.velocity.normalized;
            sumPolarization += Vector2.Angle(agent.rb.velocity.normalized, meanDirection);
            sumFoodIntensity += agent.foodSensoryIntensity;
            sumPredatorIntensity += agent.predatorSensoryIntensity;
        }
        globalPolarization = sumPolarization / (totalFish * 90);
        meanDirection = sumDirection / totalFish;
        meanSpeed = sumSpeed / totalFish;
        meanFoodIntensity = sumFoodIntensity / totalFish;
        meanPredatorIntensity = sumPredatorIntensity / totalFish;
    }

    public Dictionary<int, int> GetFishGroupings(List<int> newGrouping) {
        Dictionary<int, int> groupings = new Dictionary<int, int>();
        foreach (int num in newGrouping) {
            if (groupings.ContainsKey(num)) {
                groupings[num] = groupings[num] + 1;
            } else {
                groupings.Add(num, 1);
            }
        }
        return groupings;
    }

    private void FixedUpdate() {
        if (Time.timeScale == 0 || this.run_count < 0) return;
        if(this.simulationSteps >= this.maxSimulationSteps && this.maxSimulationSteps != 0){
            UnityEditor.EditorApplication.isPlaying = false;
        }
        if (this.statRecordTimer >= 10) {
            this.statistics.Add(new Statistics(
                this.simulationSteps,
                this.avgNeighbors,
                this.foodEaten, this.totalScore,
                this.totalAgentHitCount,
                this.totalWallHitCount,
                this.fishEaten,
                this.totalFish,
                this.avgFramesNearWall,
                this.agentSatiatedRatio,
                this.globalPolarization,
                this.meanSpeed,
                this.meanFoodIntensity,
                this.meanPredatorIntensity,
                Angle(this.meanDirection))
            );
            this.SaveStats();
            this.statRecordTimer = 0;
        }
        this.simulationSteps += 1;
        this.statRecordTimer += 1;
    }

    private void SaveStats() {
        string jsonToSave = JsonHelper.ToJson(this.statistics.ToArray(), true);
        System.IO.File.WriteAllText(Application.dataPath + $"/simulation_runs/run_{run_count}.json", jsonToSave);
    }

    private void Update() {
        int maximumGroupSize = 0;
        float avgGroupSize = 0f;
        UpdateAgentsSatiatedRatio();
        updateGlobalProperties();
        if (fishGroups.Count > 0) {
            int sumSize = 0;
            int currentMaxGroupSize = 0;
            foreach (int groupSize in fishGroups) {
                sumSize += groupSize;
                if (groupSize > currentMaxGroupSize) currentMaxGroupSize = groupSize;
            }
            avgGroupSize = ((float)sumSize) / totalFish;
            maximumGroupSize = currentMaxGroupSize;
        }
        scoreText.text = $"AverageScore: {totalScore / totalFish}\n" +
        $"AvgWallHitFrames: {totalWallHitCount / totalFish}\n" +
        $"AverageAgentHitFrames: {totalAgentHitCount / totalFish}\n" +
        $"AverageFramesNearWall: {avgFramesNearWall}\n" +
        $"AvgNeighborCount: {avgNeighbors}\n" +
        $"AvgGroupSize: {avgGroupSize}\n" +
        $"MaxGroupSize: {maximumGroupSize}\n" +
        $"TotalFishEaten: {fishEaten}\n" +
        $"TotalFoodEaten: {foodEaten}\n" +
        $"AgentSatiatedRatio: {agentSatiatedRatio}\n" +
        $"GlobalPolarization: {globalPolarization}\n" +
        $"MeanSpeed: {meanSpeed}\n" + 
        $"Simulation Steps: {this.simulationSteps}\n";
        Dictionary<int, int> groupings = GetFishGroupings(fishGroups);
        foreach (KeyValuePair<int, int> entry in groupings) {
            scoreText.text += $"\ngroup of {entry.Key} : {entry.Value}";
        }

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.

        if ((Time.frameCount % 100) == 0) {
            m_Recorder.Add("Agent/avgGroupSize", avgGroupSize);
            m_Recorder.Add("Agent/maximumGroupSize", maximumGroupSize);
            m_Recorder.Add("Agent/AverageScore", totalScore / totalFish);
            m_Recorder.Add("Agent/TotalWallHit", totalWallHitCount);
            m_Recorder.Add("Agent/TotalAgentHit", totalAgentHitCount);
            m_Recorder.Add("Agent/avgNeighbors", avgNeighbors);
            m_Recorder.Add("Agent/FoodEaten", foodEaten);
            m_Recorder.Add("Agent/TotalTimesEatenByPredator", fishEaten);
            m_Recorder.Add("Agent/AvgFramesNearWall", avgFramesNearWall);
            m_Recorder.Add("Agent/AgentsSatiatedRatio", agentSatiatedRatio);
            m_Recorder.Add("Agent/GlobalPolarization", globalPolarization);
            m_Recorder.Add("Agent/MeanSpeed", meanSpeed);
        }
    }

    public static float Angle(Vector2 vector2) {
        return 360f - (Mathf.Atan2(vector2.x, vector2.y) * Mathf.Rad2Deg * Mathf.Sign(vector2.x));
    }
}
