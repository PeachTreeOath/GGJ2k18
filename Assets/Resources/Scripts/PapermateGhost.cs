using System.Collections;
using System.Collections.Generic;
using System.IO; 
using UnityEngine;

public class PapermateGhost : MonoBehaviour {

    // Copied from papermate body, probably shouldn't be
    private Sprite leftSpriteOff;
    private Sprite rightSpriteOff;
    private Sprite leftSpriteOn;
    private Sprite rightSpriteOn;
    private Sprite leftSpriteReady;
    private Sprite rightSpriteReady;
    private LineRenderer lineRenderer;

    private FrameData[] frames;
    private int currTimestamp = 0;
    private int lastTimestamp;
    private int frameIndex = 0;
    private bool playing = false;
    
    public string animationFilename;
    public int startAt;
    public int endAt;
    public bool looping = true;

    public SpriteRenderer leftSprite;
    public SpriteRenderer rightSprite;

	void Start () {
        lineRenderer = GetComponent<LineRenderer>();

        // Copied from papermate body, probably shouldn't be
        leftSpriteOff = Resources.Load<Sprite>("Textures/leftButtonOff");
        rightSpriteOff = Resources.Load<Sprite>("Textures/rightButtonOff");
        leftSpriteOn = Resources.Load<Sprite>("Textures/leftButtonOn");
        rightSpriteOn = Resources.Load<Sprite>("Textures/rightButtonOn");
        leftSpriteReady = Resources.Load<Sprite>("Textures/leftButtonReady");
        rightSpriteReady = Resources.Load<Sprite>("Textures/rightButtonReady");

		if(Application.isPlaying && LoadAnimation()) {
            PlayAnimation();
        }
	}
	
	void Update () {
        if (!playing) return;

        // This is a janky way of doing this, but just let it tick over the top and THEN reset
        if(currTimestamp >= lastTimestamp) {
            if(looping) {
                PlayAnimation();
            } else {
                playing = false;
            }
        }

        var dtms = Mathf.FloorToInt(Time.deltaTime * 1000);
        currTimestamp += dtms;

        while (frameIndex < frames.Length - 1 && frames[frameIndex].timestamp < currTimestamp) frameIndex++;

        // No lerping for now
        CopyFromFrame(frames[frameIndex]);
	}

    private void PlayAnimation() {
        currTimestamp = startAt;
        frameIndex = 0;

        // Find a starting frame
        while (frameIndex < frames.Length && frames[frameIndex].timestamp < currTimestamp) frameIndex++;

        if(frameIndex >= frames.Length) {
            playing = false;
        } else {
            playing = true;
            // Move back one frame if we've overshot
            if(frameIndex > 0 && frames[frameIndex].timestamp > currTimestamp) {
                frameIndex--;
            }

            CopyFromFrame(frames[frameIndex]);
        }
    }

    private void CopyFromFrame(FrameData frame) {
        lineRenderer.positionCount = frame.vertices.Length;
        lineRenderer.SetPositions(frame.vertices);
        leftSprite.sprite = frame.leftActive ? leftSpriteOn : leftSpriteOff;
        rightSprite.sprite = frame.rightActive ? rightSpriteOn : rightSpriteOff;

        var offset = new Vector3(0, 0.5f, -2);
        leftSprite.transform.position = frame.vertices[0] + offset;
        rightSprite.transform.position = frame.vertices[frame.vertices.Length - 1] + offset;
    }

    // This is likely *not* web compatible and will require revisiting
    private bool LoadAnimation() {
        var filename = string.IsNullOrEmpty(animationFilename) ? "animation.json" : animationFilename;
        var path = Application.dataPath + "/StreamingAssets/" + filename;

        if( File.Exists(path) ) {
            var dataAsJson = File.ReadAllText(path);
            var animationData = JsonUtility.FromJson<AnimationData>(dataAsJson);

            frames = animationData.frames;

            if(endAt < 0) {
                lastTimestamp = frames[frames.Length - 1].timestamp;
            } else {
                lastTimestamp = Mathf.Min(endAt, frames[frames.Length - 1].timestamp);
            }

            return true;
        } else {
            Debug.LogError(string.Format("The animation \"{0}\" does not exist!", path));
            return false;
        }
    }
}
