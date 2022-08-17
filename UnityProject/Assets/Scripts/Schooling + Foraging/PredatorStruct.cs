using UnityEngine;
public struct VisiblePredator {
    public VisiblePredator(Predator predatorComponent) {
        PredatorComponent = predatorComponent;
    }
    public Predator PredatorComponent { get; }
    public Vector2 GetRelativePos(Transform transform) {
        return transform.InverseTransformPointUnscaled(PredatorComponent.transform.position);
    }
    public Vector2 GetRelativeVelocity(Transform transform) {
        return transform.InverseTransformVector(PredatorComponent.rb.velocity);
    }
}