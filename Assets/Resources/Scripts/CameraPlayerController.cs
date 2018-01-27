using UnityEngine;

/// <summary>
/// Simple camera controller to follow the player.
/// </summary>
public class CameraPlayerController : MonoBehaviour
{
    public GameObject mainPlayer;
    public float zRange = -10f;
    public float cameraSpeed = 75f;
	
	// Update is called once per frame
	void Update ()
    {
        if (mainPlayer != null)
        {
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, zRange);
            Vector3 targetPos = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, zRange);
            transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * cameraSpeed);
        }
	}
}
