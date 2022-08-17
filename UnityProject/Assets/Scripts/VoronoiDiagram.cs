using UnityEngine;
using System.Collections.Generic;
using csDelaunay;
using System.Linq;

public class VoronoiDiagram : MonoBehaviour {
    public List<FishSFAgent> selectedFish = new List<FishSFAgent>();
    private FishTrainer m_FishTrainer;

    void Start() {
        m_FishTrainer = FindObjectOfType<FishTrainer>();
    }

    private void Update() {
    }

    private void OnDrawGizmos() {
        Vector2 bottomLeft = new Vector2(1000000, 1000000);
        Vector2 topRight = new Vector2(-1000000, -1000000);
        List<Vector2f> tempPoints = new List<Vector2f>();
        if (!m_FishTrainer) return;
        if (m_FishTrainer.renderVoronoiSelected) {
            foreach (FishSFAgent agent in selectedFish) {
                Vector2 VoronoiOrigin = new Vector2(agent.transform.position.x - 50f, agent.transform.position.y - 50f);
                foreach (NeighborFish neighborFish in agent.neighborFishes) {
                    Vector2 pos = neighborFish.FishComponent.transform.position;
                    Vector2f newPoint = new Vector2f(pos.x, pos.y);
                    if (!PointExists(newPoint, tempPoints)) tempPoints.Add(newPoint);
                }
                if (VoronoiOrigin.x < bottomLeft.x) bottomLeft.x = VoronoiOrigin.x;
                if (VoronoiOrigin.y < bottomLeft.y) bottomLeft.y = VoronoiOrigin.y;
                if (VoronoiOrigin.x > topRight.x) topRight.x = VoronoiOrigin.x;
                if (VoronoiOrigin.y > topRight.y) topRight.y = VoronoiOrigin.y;
            }
        }

        Rectf bounds = new Rectf(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x + 100f, topRight.y - bottomLeft.y + 100f);
        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        Voronoi voronoi = new Voronoi(tempPoints, bounds);

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        Dictionary<Vector2f, Site> sites = voronoi.SitesIndexedByLocation;
        List<Edge> edges = voronoi.Edges;

        if (edges.Count > 1) {
            Gizmos.color = Color.yellow;
            foreach (Edge edge in edges) {
                // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
                if (edge.ClippedEnds == null) continue;
                Vector2f start = edge.ClippedEnds[LR.LEFT];
                Vector2f end = edge.ClippedEnds[LR.RIGHT];
                Gizmos.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
            }
        }
    }

    public bool PointExists(Vector2f targetPoint, List<Vector2f> pointList) {
        bool exists = false;
        foreach (Vector2f point in pointList) {
            exists = exists || targetPoint.DistanceSquare(point) < 0.01;
        }
        return exists;
    }
}

