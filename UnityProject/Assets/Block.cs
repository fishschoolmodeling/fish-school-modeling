using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
    // Start is called before the first frame update
    public List<FishSFAgent> fishInBlock = new List<FishSFAgent>();
    public List<Predator> predatorsInBlock = new List<Predator>();
    public int blockXPos;
    public int blockYPos;
    public SpriteRenderer spriteRenderer;
    void Start() {
        spriteRenderer = transform.Find("BlockSprite").GetComponent<SpriteRenderer>();
    }
}
