using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour {

    public float spawnRateInSecs;
    public int maxBalls;
    public GameObject ballPrefab;

    private int currentBalls;
    private float timeSinceLastSpawn;
    

	// Use this for initialization
	void Start () {
        currentBalls = 0;
        timeSinceLastSpawn = 0;
	}
	
	// Update is called once per frame
	void Update () {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnRateInSecs && currentBalls < maxBalls)
        {
            timeSinceLastSpawn = 0;
            currentBalls++;

            GameObject ball = Instantiate<GameObject>(ballPrefab, transform);


        }
	}
}
