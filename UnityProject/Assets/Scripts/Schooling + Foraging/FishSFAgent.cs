using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;
using System.Linq;
using csDelaunay;
using UnityEditor;

public class FishSFAgent : Agent {

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    [NonSerialized]
    private BufferSensorComponent[] m_BufferSensors;
    [NonSerialized]
    private FishTrainer m_FishTrainer;
    [NonSerialized]
    public Rigidbody2D rb;

    [Header("Abilities (edit in FishTrainer)")]
    [field: SerializeField, ReadOnlyField]
    private float steerStrength;
    [field: SerializeField, ReadOnlyField]
    private float maxSpeed;
    [field: SerializeField, ReadOnlyField]
    private float minSpeed;
    [field: SerializeField, ReadOnlyField]
    private float accelerationConstant;
    [field: SerializeField, ReadOnlyField]
    private float neighborSensorRadius;
    [field: SerializeField, ReadOnlyField]
    private float predatorSensorRadius;
    private FishTrainer.Rewards rewards;

    [Header("Statistics")]
    [field: SerializeField, ReadOnlyField]
    private float score = 0;
    [field: SerializeField, ReadOnlyField]
    private float ApparentSpeed = 0f;
    [field: SerializeField, ReadOnlyField]
    private float accelerationInput = 0f;
    [field: SerializeField, ReadOnlyField]
    private int neighborCount = 0;
    [field: SerializeField, ReadOnlyField]
    private int foodEaten = 0;
    [field: SerializeField, ReadOnlyField]
    public float foodSensoryIntensity = 0f;
    [field: SerializeField, ReadOnlyField]
    public float predatorSensoryIntensity = 0f;
    [field: SerializeField, ReadOnlyField]
    private bool foodVisible = false;
    [field: SerializeField, ReadOnlyField]
    private bool predatorVisible = false;
    [field: SerializeField, ReadOnlyField]
    private int hungerCounter = 0;
    [field: SerializeField, ReadOnlyField]
    public int stomach = 0;
    [field: SerializeField, ReadOnlyField]
    private int satiateDuration = 10;
    [field: SerializeField, ReadOnlyField]
    private int satiateDecay = 40;

    [Header("Neighbors")]
    [field: SerializeField, ReadOnlyField]
    public List<NeighborFish> neighborFishes = new List<NeighborFish>();

    public List<VisiblePredator> visiblePredators = new List<VisiblePredator>();

    [NonSerialized]
    private RayPerceptionSensorComponent2D foodSensorComponent;
    [NonSerialized]
    private RayPerceptionSensorComponent2D wallSensorComponent;
    // [NonSerialized]
    // private RayPerceptionSensorComponent2D m_PredatorSensorComponent;
    [NonSerialized]
    private EnvironmentParameters environmentParameters;
    [NonSerialized]
    private SpriteRenderer spriteRenderer;
    [NonSerialized]
    public Block block;
    [NonSerialized]
    public FishTankSF tank;
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    public bool renderVoronoi = false;
    private BufferSensorComponent fishBufferSensor;
    private VoronoiDiagram voronoiDiagram;

    public override void Initialize() {
        m_FishTrainer = FindObjectOfType<FishTrainer>();
        environmentParameters = Academy.Instance.EnvironmentParameters;
        rb = GetComponent<Rigidbody2D>();
        // m_BufferSensors = GetComponents<BufferSensorComponent>();
        fishBufferSensor = GetComponent<BufferSensorComponent>();
        RayPerceptionSensorComponent2D[] sensorComponents = GetComponents<RayPerceptionSensorComponent2D>();
        voronoiDiagram = FindObjectOfType<VoronoiDiagram>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        foodSensorComponent = sensorComponents[0];
        wallSensorComponent = sensorComponents[1];
        this.steerStrength = m_FishTrainer.agentSteerStrength;
        this.maxSpeed = m_FishTrainer.agentMaxSpeed;
        this.minSpeed = m_FishTrainer.agentMinSpeed;
        this.accelerationConstant = m_FishTrainer.agentAccelerationConstant;
        this.neighborSensorRadius = m_FishTrainer.agentNeighborSensorRadius;
        this.predatorSensorRadius = m_FishTrainer.agentPredatorSensorRadius;
        this.satiateDuration = m_FishTrainer.satiateDuration;
        this.satiateDecay = m_FishTrainer.satiateDecay;
        this.rewards = m_FishTrainer.rewards;
    }

    public override void OnEpisodeBegin() {
        SetResetParameters();
    }

    public void SetResetParameters() {
        ResetPosition();
        foodEaten = 0;
    }
    private void ResetPosition() {
        tank.ResetAgent(this.transform);
        rb.velocity = (maxSpeed + minSpeed) * 0.5f * RandomUnitVector();
    }

    public override void CollectObservations(VectorSensor sensor) {
        (this.neighborFishes, this.visiblePredators) = ScanEnvironment();

        //neighborCount
        if (this.neighborFishes.Count < this.neighborCount) {
            float difference = Math.Abs(neighborCount - neighborFishes.Count);
            //negative reward punishment for losing neighbors
            float loseNeighborTotalReward = difference * rewards.loseNeighborReward;
            this.AddScoreReward(loseNeighborTotalReward);
        }
        neighborCount = neighborFishes.Count;

        int i = 0;
        foreach (NeighborFish neighborFish in neighborFishes) {
            if (i > 23) break;
            FishSFAgent agent = neighborFish.FishComponent;
            Vector3 pos = neighborFish.GetRelativePos(this.transform);
            Vector3 velocity = neighborFish.GetRelativeVelocity(this.transform);
            float nPosX = Mathf.Clamp(pos.x / neighborSensorRadius, -1, 1);
            float nPosY = Mathf.Clamp(pos.y / neighborSensorRadius, -1, 1);
            float nVelX = Mathf.Clamp(velocity.x / maxSpeed, -1, 1);
            float nVelY = Mathf.Clamp(velocity.y / maxSpeed, -1, 1);
            float foodIntensity = Mathf.Clamp(agent.foodSensoryIntensity, 0, 1);
            float predatorIntensity = Mathf.Clamp(agent.predatorSensoryIntensity, 0, 1);
            // float nPosX = pos.x;
            // float nPosY = pos.y;
            // float nVelX = velocity.x;
            // float nVelY = velocity.y;
            // float foodIntensity = agent.foodSensoryIntensity;
            // float predatorIntensity = agent.predatorSensoryIntensity;
            float[] neighborFishData = { nPosX, nPosY, nVelX, nVelY, foodIntensity, predatorIntensity };
            fishBufferSensor.AppendObservation(neighborFishData);
            i++;
        }

        Vector2 predatorPos = new Vector2(0, 0);
        Vector2 predatorVel = new Vector2(0, 0);
        if (visiblePredators.Count > 0) {
            predatorPos = visiblePredators[0].GetRelativePos(this.transform);
            predatorVel = visiblePredators[0].GetRelativeVelocity(this.transform);
            if (predatorPos.magnitude > 0)
                this.AddScoreReward((1 / (predatorPos.magnitude / this.predatorSensorRadius)) * rewards.predatorDistanceReward);
        }
        Vector2 localVelocity = transform.InverseTransformVector(rb.velocity);
        sensor.AddObservation(Mathf.Clamp(localVelocity.x / maxSpeed, -1, 1));
        sensor.AddObservation(Mathf.Clamp(localVelocity.y / maxSpeed, -1, 1));
        sensor.AddObservation(Mathf.Clamp(stomach / this.satiateDuration, 0, 1));

        sensor.AddObservation(Mathf.Clamp(predatorPos.x / predatorSensorRadius, -1, 1));
        sensor.AddObservation(Mathf.Clamp(predatorPos.y / predatorSensorRadius, -1, 1));
        sensor.AddObservation(Mathf.Clamp(predatorVel.x / m_FishTrainer.predatorChaseSpeed, -1, 1));
        sensor.AddObservation(Mathf.Clamp(predatorVel.y / m_FishTrainer.predatorChaseSpeed, -1, 1));

        // sensor.AddObservation(localVelocity.x);
        // sensor.AddObservation(localVelocity.y);

        // sensor.AddObservation(predatorPos.y);
        // sensor.AddObservation(predatorPos.x);
        // sensor.AddObservation(predatorVel.x);
        // sensor.AddObservation(predatorVel.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
        if (stomach > 0) {
            this.AddScoreReward(rewards.seeNeighborReward * this.neighborCount);
        } else {
            this.AddScoreReward(rewards.idleReward);
        }
        MoveAgent(actionBuffers);
    }

    public void MoveAgent(ActionBuffers actionBuffers) {
        var continuousActions = actionBuffers.ContinuousActions;
        this.MoveSteer(Mathf.Clamp(continuousActions[0], -1f, 1f));
        this.MoveForward(Mathf.Clamp(continuousActions[1], -1f, 1f));
        this.accelerationInput = Mathf.Clamp(continuousActions[1], -1f, 1f);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    }

    public void MoveForward(float input) {
        rb.velocity += new Vector2(transform.up.x, transform.up.y) * accelerationConstant * input;
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) // slow it down
        {
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
        } else if (rb.velocity.sqrMagnitude < minSpeed * minSpeed) {
            rb.velocity = rb.velocity.normalized * minSpeed;
        }
    }

    public void MoveSteer(float input) {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * input;
        rb.AddForce(steerLeftForce);
    }

    public void Update() {
        if ((Time.frameCount % 100) == 0) {
            m_FishTrainer.UpdateNeighborCount(neighborCount);
        }
        this.spriteRenderer.color = new Color(1, (30 - this.rb.velocity.magnitude) / 20, (30 - this.rb.velocity.magnitude) / 20, 1);
        this.ApparentSpeed = rb.velocity.magnitude;
        // if (this.renderVoronoi) {
        bool inVoronoiList = voronoiDiagram.selectedFish.Contains(this);
        if (Selection.gameObjects.Contains(this.gameObject)) {
            if (!inVoronoiList) voronoiDiagram.selectedFish.Add(this);
        } else {
            if (inVoronoiList) voronoiDiagram.selectedFish.Remove(this);
        }
    }

    public void FixedUpdate() {
        if (hungerCounter >= this.satiateDecay) {
            hungerCounter = 0;
            if (stomach > 0) {
                stomach--;
            } else {
                stomach = 0;
            }
        } else {
            hungerCounter++;
        }
    }

    private (List<NeighborFish>, List<VisiblePredator>) ScanEnvironment() {
        if (!block) {
            // Debug.Log("block not found");
            return (new List<NeighborFish>(), new List<VisiblePredator>());
        }

        if (tank.fishes == null) return (new List<NeighborFish>(), new List<VisiblePredator>());

        Block[] blocksToScan = new Block[9];
        int blockCount = 0;
        for (int i = block.blockXPos - 1; i <= block.blockXPos + 1; i++) {
            for (int j = block.blockYPos - 1; j <= block.blockYPos + 1; j++) {
                if (i < tank.gridBlocks.GetLength(0) && j < tank.gridBlocks.GetLength(0) && i >= 0 && j >= 0) {
                    blocksToScan[blockCount] = tank.gridBlocks[i, j];
                    blockCount++;
                }
            }
        }

        List<FishSFAgent> fishToScan = blocksToScan.Where(block => block != null).SelectMany(block => block.fishInBlock).Distinct().ToList();
        List<Predator> predatorsToScan = blocksToScan.Where(block => block != null).SelectMany(block => block.predatorsInBlock).Distinct().ToList();

        List<NeighborFish> tempNeighborFishes = new List<NeighborFish>();
        List<VisiblePredator> tempVisiblePredators = new List<VisiblePredator>();

        float maxFoodIntensity = 0f;
        float maxPredatorIntensity = 0f;

        // foreach (FishSFAgent fish in tank.fishes) {
        foreach (FishSFAgent fish in fishToScan) {
            Vector3 neighborFishPosition = fish.transform.position;
            Vector3 offset = transform.InverseTransformPoint(neighborFishPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if (sqrtDst <= neighborSensorRadius * neighborSensorRadius) {
                if (fish.foodSensoryIntensity >= maxFoodIntensity) maxFoodIntensity = fish.foodSensoryIntensity;
                if (fish.predatorSensoryIntensity >= maxPredatorIntensity) maxPredatorIntensity = fish.predatorSensoryIntensity;
                NeighborFish neighbor = new NeighborFish(fish);
                tempNeighborFishes.Add(neighbor);
            }
        }

        bool tempPredatorVisible = false;
        // foreach (Predator predator in tank.predators) {
        foreach (Predator predator in tank.predators) {
            Vector3 visiblePredatorPosition = predator.transform.position;
            Vector3 offset = transform.InverseTransformPoint(visiblePredatorPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if (sqrtDst <= predatorSensorRadius * predatorSensorRadius) {
                Vector2 velocity = transform.InverseTransformVector(predator.rb.velocity);
                VisiblePredator visiblePredator = new VisiblePredator(predator);
                tempVisiblePredators.Add(visiblePredator);
                tempPredatorVisible = true;
            }
        }

        RayPerceptionSensor foodRaySensor = foodSensorComponent.RaySensor;

        bool tempFoodVisible = false;
        foreach (RayPerceptionOutput.RayOutput output in foodRaySensor.RayPerceptionOutput.RayOutputs) {
            if (output.HitGameObject) {
                if (output.HitGameObject.CompareTag("food")) {
                    tempFoodVisible = true;
                }
            }
        }

        RayPerceptionSensor WallRaySensor = wallSensorComponent.RaySensor;

        foreach (RayPerceptionOutput.RayOutput output in WallRaySensor.RayPerceptionOutput.RayOutputs) {
            if (output.HitGameObject) {
                if (output.HitGameObject.CompareTag("wall") && output.HitFraction < 0.1) {
                    this.m_FishTrainer.UpdateFramesNearWall();
                }
            }
        }
        this.foodVisible = tempFoodVisible;

        this.predatorVisible = tempPredatorVisible;

        if (!this.foodVisible) {
            this.foodSensoryIntensity = maxFoodIntensity > 0.1 ? maxFoodIntensity * 0.8f : 0;
        } else {
            this.foodSensoryIntensity = 1f;
        }
        if (!this.predatorVisible) {
            this.predatorSensoryIntensity = maxPredatorIntensity > 0.1 ? maxPredatorIntensity * 0.8f : 0;
        } else {
            this.predatorSensoryIntensity = 1f;
        }

        tempNeighborFishes.OrderBy(a => a.GetRelativePos(this.transform).sqrMagnitude);
        tempVisiblePredators.OrderBy(a => a.GetRelativePos(this.transform).sqrMagnitude);

        return (tempNeighborFishes, tempVisiblePredators);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.RightArrow)) {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            continuousActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            continuousActionsOut[0] = -1;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            continuousActionsOut[1] = -1;
        }
    }

    public Vector2 RandomUnitVector() {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    public void Satiate() {
        foodEaten++;
        stomach = this.satiateDuration;
    }
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("food")) {
            collision.gameObject.GetComponent<FoodLogicSF>().OnEaten();
            Satiate();
            m_FishTrainer.foodEaten += 1;
            this.AddScoreReward(rewards.eatReward);
        }
    }
    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("wall")) {
            m_FishTrainer.totalWallHitCount += 1;
            this.AddScoreReward(rewards.wallCrashReward);
        }

        if (collision.gameObject.CompareTag("agent")) {
            m_FishTrainer.totalAgentHitCount += 1;
            this.AddScoreReward(rewards.neighborCrashReward);
        }
    }
    public void OnEaten() {
        this.AddScoreReward(rewards.onEatenReward);
        this.m_FishTrainer.fishEaten += 1;
        this.ResetPosition();
    }
    void AddScoreReward(float reward) {
        this.score += reward;
        m_FishTrainer.totalScore += reward;
        AddReward(reward);
    }
    private void OnDrawGizmosSelected() {
        FishTrainer trainer = FindObjectOfType<FishTrainer>();
        if (trainer.renderNeighborRaySelected) {
            foreach (NeighborFish fish in neighborFishes) {
                Vector3 target = transform.TransformPoint(fish.GetRelativePos(this.transform));
                float intensity = ((neighborSensorRadius - Vector2.Distance(new Vector2(0, 0), fish.GetRelativePos(this.transform))) / neighborSensorRadius);
                Gizmos.color = new Color(0, 1, 1, intensity);
                Gizmos.DrawLine(transform.position, target);
            }
        }
        if (trainer.renderNeighborSensorSelected) {
            //draw neighbor sensor radius
            Gizmos.color = Color.blue;
            float corners = 30; // How many corners the circle should have
            float size = neighborSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.right * size; // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            while (angle <= 360) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                // Gizmos.DrawSphere(nextPosition, 1);

                lastPosition = nextPosition;
            }
        }

        if (trainer.renderPredatorSensorSelected) {
            //draw neighbor sensor radius
            Gizmos.color = Color.yellow;
            float corners = 30; // How many corners the circle should have
            float size = predatorSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.right * size; // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            while (angle <= 360) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                // Gizmos.DrawSphere(nextPosition, 1);

                lastPosition = nextPosition;
            }
        }

        if (trainer.renderPredatorRaySelected) {
            foreach (VisiblePredator visiblePredator in visiblePredators) {
                Vector3 target = transform.TransformPoint(visiblePredator.GetRelativePos(this.transform));
                float intensity = ((predatorSensorRadius - Vector2.Distance(new Vector2(0, 0), visiblePredator.GetRelativePos(this.transform))) / predatorSensorRadius);
                Gizmos.color = new Color(1, 0, 0, intensity);
                Gizmos.DrawLine(transform.position, target);
            }
        }

        if (trainer.renderVisionConeSelected) {
            //draw food ray sensor
            Gizmos.color = Color.yellow;
            float corners = 30; // How many corners the circle should have
            float size = neighborSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.TransformDirection((Quaternion.Euler(0, 0, 18f) * new Vector3(1, 0, 0)) * 100); // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            Gizmos.DrawLine(transform.position, lastPosition);
            while (angle <= 70 * 2) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                lastPosition = nextPosition;
            }
            Gizmos.DrawLine(transform.position, lastPosition);
        }
    }

    private void OnDrawGizmos() {
        if (m_FishTrainer.renderNeighborRayAll) {
            Gizmos.color = new Color(0, 1, 1, 1);
            foreach (NeighborFish fish in neighborFishes) {
                Vector3 target = transform.TransformPoint(fish.GetRelativePos(this.transform));
                Gizmos.DrawLine(transform.position, target);
            }
        }
    }
    private void OnMouseDown() {
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho = gameObject;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followName = gameObject.name;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().framesFollowed = 0;
    }
}