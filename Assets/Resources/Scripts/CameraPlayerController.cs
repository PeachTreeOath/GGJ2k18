using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Simple camera controller to follow the player.
/// </summary>
public class CameraPlayerController : MonoBehaviour
{
    public GameObject mainPlayer; // the player to follow on screen
    public Font gucciGoldFont; // the font to use for the UI

    public bool updateTime = true; // For debug: stops timer when false
    public bool doThatDisco = true; // Show colored vignette to beat
    public bool feelinTheBeat = true; // Zoom camera to beat

    public float zRange = -10f; // distance from the player content
    public float cameraSpeed = 10f; // how responsive the camera should be to motion
    public float beatsPerMinute = 128f; // music speed
    public float beatResizePercent = .99f; // the zoomed in camera size as percentage of original
    public float vignetteOpacityPercent = 1.00f; // how much color to show
    public float vignetteIntenityPercent = .30f; // how far the vignette should encroach

    private float _timer = 0f; // keeps track of the time since the game began
    private float _beatTimer = 0f; // keeps track of when it's time for the beat
    private float _secPerBeat; // beat frequency
    private float _fullCamSize; // the original camera size
    private float _zoomedCamSize; // the size to show when zooming with the beat

    private PostProcessVolume _volume; // triggers vignette changes
    private AudioSource _musicSource; // cue music
    private Vignette _vignette; // shows colored outline on camera
    private GUIStyle _style; // used to style text for UI

    void Start()
    {
        _musicSource = GameObject.Find("Level").GetComponent<AudioSource>();

        _vignette = ScriptableObject.CreateInstance<Vignette>();
        _vignette.enabled.Override(true);
        _vignette.intensity.Override(0f);
        _vignette.color.Override(new Color(Random.value, Random.value, Random.value));

        _volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, _vignette);
        _secPerBeat = 60f / beatsPerMinute;

        _style = new GUIStyle();
        _style.fontSize = 20;
        _style.font = gucciGoldFont;
        _style.normal.textColor = new Color(0.15f, 0.15f, 0.15f); // dark grey

        _fullCamSize = Camera.main.orthographicSize;
        _zoomedCamSize = _fullCamSize * beatResizePercent;
    }

    // Update is called once per frame
    void Update()
    {
        // move camera to player smoothly
        if (mainPlayer != null)
        {
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, zRange);
            Vector3 targetPos = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, zRange);
            transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * cameraSpeed);
        }

        // we have music and want effects
        if (_musicSource.isPlaying && (doThatDisco || feelinTheBeat))
        {
            _beatTimer += Time.deltaTime;
            if (_beatTimer >= _secPerBeat)
            {
                _beatTimer %= _secPerBeat; // keeps it on beat without relying on framerate
                if (doThatDisco)
                {
                    _vignette.intensity.value = vignetteIntenityPercent;
                    _vignette.color.value = new Color(Random.value, Random.value,
                                                      Random.value, vignetteOpacityPercent);
                }
                if (feelinTheBeat)
                {
                    Camera.main.orthographicSize = _zoomedCamSize;
                    StartCoroutine(ZoomOut()); // resize to original after instant in
                }

            }
        }

        // enable/disable visual effects
        if (Input.GetKeyDown(KeyCode.B))
        {
            feelinTheBeat = !feelinTheBeat;
            doThatDisco = !doThatDisco;
        }

        if (updateTime)
            _timer += Time.deltaTime;
    }

    IEnumerator ZoomOut()
    {
        float startVignette = vignetteIntenityPercent;
        float start = _zoomedCamSize;
        float goal = _fullCamSize;
        float length = 0.35f;
        float time = 0f;
        while (Camera.main.orthographicSize < _fullCamSize)
        {
            time += Time.deltaTime;
            float t = Mathf.Sin(time * Mathf.PI * 0.5f);
            Camera.main.orthographicSize = Mathf.Lerp(start, goal, t / length);
            _vignette.intensity.value = Mathf.Lerp(startVignette, 0f, t / length);
            yield return null;
        }
    }

    void OnGUI()
    {
        int minutes = (int)_timer / 60; // truncates by integer division
        int seconds = (int)_timer % 60; // remainder after dividing by 60
        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        GUI.Label(new Rect(15, 10, 250, 100), niceTime, _style);
    }

    public float GetTime()
    {
        return _timer; // accessor so that the value is internally controlled
    }
}
