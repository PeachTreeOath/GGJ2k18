using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscalatorSpawn : MonoBehaviour {

	public GameObject stepPrefab;
	public float stairSpeed = 2;
	private List<GameObject> steps = new List<GameObject>();
	public GameObject first;
	public GameObject last;
	
	// Use this for initialization
	void Start () {
		//Generate the initial steps
		//steps.Add(Instantiate(step, new Vector3(step.transform.position.x, step.transform.position.y, 0), Quaternion.identity));
		steps.Add(Instantiate(stepPrefab, first.transform.position, Quaternion.identity));
		for (int i = 0; i < 7; i++){
			GenerateStep();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Move the steps
		foreach (GameObject step in steps)
		{
			step.transform.Translate(new Vector3(stairSpeed * Time.deltaTime, -stairSpeed * Time.deltaTime), 0);
		}
		
		//Destroy old steps
		if(steps[0].transform.position.x > first.transform.position.x)
		{
				GameObject removed = steps[0];
				steps.Remove(removed);
				Destroy(removed);
				//steps[0].transform.position = new Vector3(0, steps[0].transform.position.y, steps[0].transform.position.z);
		}
		
		//Create new steps
		Debug.Log(steps[steps.Count - 1].transform.position.x);
		Debug.Log(last.transform.position.x);
		if(steps[steps.Count - 1].transform.position.x > last.transform.position.x + last.GetComponent<Renderer>().bounds.size.x)
		{
			GenerateStep();
		}
		
		//Generate a new step if needed
	}
	
	void GenerateStep () {
		//Create a new GameObject at the selected location
		GameObject previous = steps[steps.Count-1];
		GameObject current = Instantiate(stepPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		
		Vector3 currentLocation = new Vector3(previous.transform.position.x - current.GetComponent<Renderer>().bounds.size.x,
									          previous.transform.position.y + current.GetComponent<Renderer>().bounds.size.y,
									          0);
		current.transform.position = currentLocation;
		steps.Add(current);
	}
}
