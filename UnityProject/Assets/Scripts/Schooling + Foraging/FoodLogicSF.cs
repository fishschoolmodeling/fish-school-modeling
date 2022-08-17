using UnityEngine;

public class FoodLogicSF : MonoBehaviour
{
    public bool respawn;
    public FoodClusterSF myCluster;

    public void OnEaten()
    {
        myCluster.totalFoodAmount -= 1;   
        if (respawn)
        {
            transform.position = new Vector3(Random.Range(-myCluster.width/2, myCluster.width/2),
                Random.Range(-myCluster.height/2, myCluster.height/2),
                0f) + myCluster.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
