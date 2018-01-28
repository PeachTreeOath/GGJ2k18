using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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

    PostProcessVolume volume;
    Vignette vignette;

    void Start()
    {
        musicSource = GameObject.Find("Level").GetComponent<AudioSource>();
        vignette = ScriptableObject.CreateInstance<Vignette>();
        vignette.enabled.Override(true);
        vignette.intensity.Override(0f);
        vignette.color.Override(new Color(Random.value, Random.value, Random.value));

        volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, vignette);
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
                    Camera.main.orthographicSize = 4.8f;
                    vignette.intensity.value = 0.6f;
                    vignette.color.value = new Color(Random.value, Random.value, Random.value, 0.3f);
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
        float startVignette = vignette.intensity.value;
        float goal = 5f;
        float length = 0.35f;
        float time = 0f;
        while (Camera.main.orthographicSize < 5)
        {
            time += Time.deltaTime;
            float t = Mathf.Sin(time * Mathf.PI * 0.5f);
            Camera.main.orthographicSize = Mathf.Lerp(start, goal, t / length);
            vignette.intensity.value = Mathf.Lerp(startVignette, 0f, t / length);
            yield return null;
        }
    }
}
