using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Predator : MonoBehaviour {
    [Header("Statistics")]
    [field: SerializeField, ReadOnlyField]
    private int fishEaten = 0;
    [field: SerializeField, ReadOnlyField]
    private float apparentSpeed = 0;
    [field: SerializeField, ReadOnlyField]
    private Transform target = null;
    [field: SerializeField, ReadOnlyField]
    private int stomach = 6;
    [field: SerializeField, ReadOnlyField]
    private bool hungry = false;
    [field: SerializeField, ReadOnlyField]
    private bool inFoodZone = false;

    [Header("Abilities (edit in FishTrainer)")]
    [field: SerializeField, ReadOnlyField]
    private float cruiseSpeed = 10f;
    [field: SerializeField, ReadOnlyField]
    private float chaseSpeed = 22f;
    [field: SerializeField, ReadOnlyField]
    private float visibleRadius = 40f;
    [field: SerializeField, ReadOnlyField]
    private float steerStrength = 5f;
    [field: SerializeField, ReadOnlyField]
    private float chaseForceMagnitude = 30f;
    [field: SerializeField, ReadOnlyField]
    private int maxStomach = 10;
    [field: SerializeField, ReadOnlyField]
    private bool swimToCluster;


    [NonSerialized]
    public Rigidbody2D rb;
    [NonSerialized]
    private int stomachCounter = 0;
    [NonSerialized]
    private FishTrainer fishTrainer;
    [NonSerialized]
    public Block block;
    [NonSerialized]
    public FishTankSF tank;

    // Start is called before the first frame update
    void Start() {
        this.rb = GetComponent<Rigidbody2D>();
        fishTrainer = FindObjectOfType<FishTrainer>();

        this.cruiseSpeed = fishTrainer.predatorCruiseSpeed;
        this.chaseSpeed = fishTrainer.predatorChaseSpeed;
        this.visibleRadius = fishTrainer.predatorVisibleRadius;
        this.steerStrength = fishTrainer.predatorSteerStrength;
        this.chaseForceMagnitude = fishTrainer.predatorChaseForceMagnitude;
        this.maxStomach = fishTrainer.predatorMaxStomach;
        this.swimToCluster = fishTrainer.predatorSwimToCluster;

        this.rb.velocity = transform.up * this.cruiseSpeed;
    }
    void FixedUpdate() {
        if (stomachCounter >= 50) {
            stomachCounter = 0;
            if (stomach > maxStomach && hungry) hungry = false;
            if (stomach > 0 && !hungry) stomach--;
            if (stomach <= 0) hungry = true;
        } else {
            stomachCounter++;
        }

        RaycastHit2D raycastHitRight = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.up + Vector2.right), 30f);
        RaycastHit2D raycastHitLeft = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.up + Vector2.left), 30f);
        Debug.DrawRay(transform.position, Vector3.Normalize(transform.TransformDirection(Vector2.up + Vector2.right)) * 30f, Color.green);
        Debug.DrawRay(transform.position, Vector3.Normalize(transform.TransformDirection(Vector2.up + Vector2.left)) * 30f, Color.green);
        bool wallHitLeft = false;
        bool wallHitRight = false;
        if (raycastHitRight.transform != null) wallHitRight = raycastHitRight.transform.tag == "wall";
        if (raycastHitLeft.transform != null) wallHitLeft = raycastHitLeft.transform.tag == "wall";
        if (wallHitLeft || wallHitRight) {
            if (wallHitLeft && wallHitRight) {
                Steer(1f);
            } else if (wallHitRight) {
                Steer(-1f);
            } else if (wallHitLeft) {
                Steer(1f);
            }
        }

        List<NeighborFish> visibleFishes = ScanEnvironment();
        if (visibleFishes.Count > 0 && hungry) {
            NeighborFish closestFish = visibleFishes.Aggregate((fish1, fish2) =>
                fish1.GetRelativePos(this.transform).sqrMagnitude < fish2.GetRelativePos(this.transform).sqrMagnitude ? fish1 : fish2);
            // Discrete Actions
            // bool onLeft = Vector3.Cross(transform.InverseTransformVector(this.rb.velocity), closestFish.GetPos()).z > 0;
            // bool onRight = Vector3.Cross(transform.InverseTransformVector(this.rb.velocity), closestFish.GetPos()).z < 0;
            // if(onLeft) {
            //     Debug.Log("fish on left");
            //     Steer(-1f);
            // }
            // else if(onRight) {
            //     Debug.Log("fish on right");
            //     Steer(1f);
            // }

            // Continuous Actions
            this.target = closestFish.FishComponent.transform;
            MoveTowardPoint(closestFish.GetRelativePos(this.transform));
            if (rb.velocity.magnitude > chaseSpeed) {
                rb.velocity *= 0.95f;
            }
            if (rb.velocity.magnitude < chaseSpeed) {
                rb.velocity = Vector2.ClampMagnitude(rb.velocity, chaseSpeed);
            }
        } else {
            this.target = null;
            RandomSteer((Mathf.PerlinNoise(transform.position.x * 0.03f, transform.position.y * 0.03f) * 2) - 1);
            if (rb.velocity.magnitude > cruiseSpeed) {
                rb.velocity *= 0.95f;
            }
            if (rb.velocity.magnitude < cruiseSpeed) {
                rb.velocity *= 1.05f;
                rb.velocity = Vector2.ClampMagnitude(rb.velocity, cruiseSpeed);
            }
            if (hungry && !inFoodZone && swimToCluster) {
                FishTankSF tank = transform.GetComponentInParent<FishTankSF>();
                if (tank.foodClusters.Count > 0) {
                    FoodClusterSF closestCluster = tank.foodClusters.Aggregate((cluster1, cluster2)
                        => Vector3.Distance(this.transform.position, cluster1.transform.position) < Vector3.Distance(this.transform.position, cluster2.transform.position) ? cluster1 : cluster2);
                    MoveTowardPoint(this.transform.InverseTransformPoint(closestCluster.transform.position));
                    this.target = closestCluster.transform;
                }
            }
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
        apparentSpeed = rb.velocity.magnitude;
    }
    public void MoveTowardPoint(Vector2 localPoint) {
        Vector2 force = Quaternion.LookRotation(Vector3.forward, rb.velocity) * localPoint.normalized * chaseForceMagnitude; // rotate direction to match current heading
        Debug.DrawRay(transform.position, force, Color.red);
        this.rb.AddForce(force);
    }
    public void Steer(float input) {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * Mathf.Clamp(input, -1f, 1f);
        this.rb.AddForce(steerLeftForce);
    }

    public void RandomSteer(float input) {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * 5 * Mathf.Clamp(input, -1f, 1f);
        this.rb.AddForce(steerLeftForce);
    }
    private List<NeighborFish> ScanEnvironment() {
        if (!block) {
            Debug.Log("block not found");
            return new List<NeighborFish>();
        }

        if (tank.fishes == null) return new List<NeighborFish>();

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

        List<NeighborFish> tempVisibleFish = new List<NeighborFish>();

        foreach (FishSFAgent fish in fishToScan) {
            Vector3 fishPosition = fish.transform.position;
            Vector3 offset = transform.InverseTransformPoint(fishPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if (sqrtDst <= this.visibleRadius * this.visibleRadius) {
                Vector2 velocity = transform.InverseTransformVector(fish.rb.velocity);
                NeighborFish visibleFish = new NeighborFish(fish);
                tempVisibleFish.Add(visibleFish);
            }
        }

        return tempVisibleFish;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("agent")) {
            stomach += 1;
            fishEaten += 1;
            collision.transform.GetComponent<FishSFAgent>().OnEaten();
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.transform.CompareTag("food_cluster")) {
            this.inFoodZone = true;
        }

    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.transform.CompareTag("food_cluster")) {
            this.inFoodZone = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        float corners = 30; // How many corners the circle should have
        float size = visibleRadius; // How wide the circle should be
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

    private void OnMouseDown() {
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho = gameObject;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followName = gameObject.name;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().framesFollowed = 0;
    }
}
