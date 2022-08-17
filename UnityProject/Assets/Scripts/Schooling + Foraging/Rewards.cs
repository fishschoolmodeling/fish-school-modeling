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