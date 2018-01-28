using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour {

    public float spawnRateInSecs;
    public int maxBalls;
    public GameObject[] ballPrefabs;
    public Vector2 startVelocity;

    private int currentBalls;
    private float timeSinceLastSpawn;
    private Queue<GameObject> balls;
    

	// Use this for initialization
	void Start () {
        currentBalls = 0;
        timeSinceLastSpawn = 0;
        balls = new Queue<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnRateInSecs)
        {
            if (currentBalls >= maxBalls)
            {
                GameObject oldestBall = balls.Dequeue();
                Destroy(oldestBall);
                currentBalls--;
            }
            timeSinceLastSpawn = 0;
            currentBalls++;
            int randPrefabIndex = Random.Range(0, ballPrefabs.Length);
            GameObject ball = Instantiate<GameObject>(ballPrefabs[randPrefabIndex], transform);
            ball.GetComponent<Rigidbody2D>().AddForce(startVelocity, ForceMode2D.Impulse);
            balls.Enqueue(ball);
        }
	}
}
