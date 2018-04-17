using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FrameData {
    public int timestamp;
    public Vector3[] vertices;
    public bool leftActive;
    public bool rightActive;
}

[System.Serializable]
public class AnimationData {
    public FrameData[] frames;
}

public class PapermateRecorder : MonoBehaviour {

    private LineRenderer lineRenderer;
    private PapermateBodyNew papermateBody;

    private bool recording = false;
    private int timestamp = 0;
    private List<FrameData> frames;
    private Vector3[] vertices;

    public string filename;

	void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        papermateBody = GetComponent<PapermateBodyNew>();

        if (frames == null) frames = new List<FrameData>();
	}
	
	void Update () {
        if ( Input.GetButtonDown("ToggleRecord") ) {
            if(!recording) {
                recording = true;
            } else {
                recording = false;
                SaveAnimationToFile();
                TruncateAnimationData();
            }
        }

        if (!recording) return;

        RecordFrame();

        var dt = Mathf.FloorToInt(Time.deltaTime * 1000);
        timestamp += dt;
	}

    private void RecordFrame() {
        var size = lineRenderer.positionCount;
        var vertices = new Vector3[size];

        lineRenderer.GetPositions(vertices);

        var frame = new FrameData();
        frame.timestamp = timestamp;
        frame.vertices = vertices;
        frame.leftActive = papermateBody.isLeftActive;
        frame.rightActive = papermateBody.isRightActive;

        frames.Add(frame);
    }

    private void SaveAnimationToFile() {

        var animation = new AnimationData();
        animation.frames = frames.ToArray();

        var filename = string.IsNullOrEmpty(this.filename) ? "animation.json" : this.filename;
        var path = Application.dataPath + "/StreamingAssets/" + filename;
        var dataAsText = JsonUtility.ToJson(animation, true);

        Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
        File.WriteAllText(path, dataAsText);
    }

    private void TruncateAnimationData() {
        frames.Clear();
        timestamp = 0;
    }
}

