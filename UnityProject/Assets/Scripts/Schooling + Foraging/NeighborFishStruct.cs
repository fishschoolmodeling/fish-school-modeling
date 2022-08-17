using UnityEngine;

public struct NeighborFish {
    public NeighborFish(FishSFAgent fishComponent) {
        FishComponent = fishComponent;
    }
    public FishSFAgent FishComponent { get; }
    public Vector3 GetRelativePos(Transform transform) {
        return transform.InverseTransformPointUnscaled(FishComponent.transform.position);
    }
    public Vector3 GetRelativeVelocity(Transform transform) {
        return transform.InverseTransformVector(FishComponent.rb.velocity);
    }
}

