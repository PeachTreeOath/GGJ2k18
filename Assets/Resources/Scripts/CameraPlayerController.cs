using System.Collections;
using UnityEngine;

/// <summary>
/// Simple camera controller to follow the player.
/// </summary>
public class CameraPlayerController : MonoBehaviour
{
    public GameObject mainPlayer;
    public float zRange = -10f;
    public float cameraSpeed = 75f;

    AudioSource musicSource;
    float beatTimer = 0f;
    float secPerBeat = 0.46875f;

    bool feelinTheBeat = false;

    void Start()
    {
        musicSource = GameObject.Find("Level").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (mainPlayer != null)
        {
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, zRange);
            Vector3 targetPos = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, zRange);
            transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * cameraSpeed);
        }

        if (musicSource.isPlaying)
        {
            beatTimer += Time.deltaTime;
            Vector3 temp = Camera.main.transform.position;
            if (beatTimer >= secPerBeat)
            {
                beatTimer += beatTimer - secPerBeat;
                beatTimer = 0f;
                if (feelinTheBeat)
                {
                    Debug.Log("feel the beat");
                    Camera.main.orthographicSize = 4.5f;
                    StartCoroutine(ZoomIn());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            feelinTheBeat = !feelinTheBeat;
        }
    }

    IEnumerator ZoomIn()
    {
        float start = Camera.main.orthographicSize;
        float goal = 5f;
        float length = 0.35f;
        float time = 0f;
        while (Camera.main.orthographicSize < 5)
        {
            time += Time.deltaTime;
            float t = Mathf.Sin(time * Mathf.PI * 0.5f);
            Camera.main.orthographicSize = Mathf.Lerp(start, goal, time / length);
            yield return null;
        }
    }
}
