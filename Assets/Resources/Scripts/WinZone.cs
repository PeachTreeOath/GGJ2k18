using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Sprite winSprite;
    public Sprite loseSprite;

    private CameraPlayerController _cpc;

    void Start()
    {
        _cpc = Camera.main.gameObject.GetComponent<CameraPlayerController>();
    }

    void OnTriggerEnter2D(Collider2D incoming)
    {
        if (incoming.GetComponent<WinTrigger>() != null)
        {
            StartVictory();
        }
    }

    private void StartVictory()
    {
		Vector3 camLoc = Camera.main.transform.position;
        PapermateBodyNew paper = GameObject.Find("papermate").GetComponent<PapermateBodyNew>();
        paper.FreezeBody();
        sprite.sprite = _cpc.GetTime() < 600f ? winSprite : loseSprite;
        sprite.enabled = true;
        sprite.transform.position = new Vector3(camLoc.x - 3f, camLoc.y, 
                                                sprite.transform.position.z);
    }
}
