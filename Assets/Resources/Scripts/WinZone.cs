﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour {

    public SpriteRenderer sprite;

    void OnTriggerEnter2D(Collider2D col)
    {
        WinTrigger win = col.GetComponent<WinTrigger>();
        if(win != null)
        {
            StartVictory();
        }
    }

    private void StartVictory()
    {
        PapermateBody paper = GameObject.Find("papermate").GetComponent<PapermateBody>();
        paper.FreezeBody();

        sprite.enabled = true;
        sprite.transform.position = new Vector3(Camera.main.transform.position.x-3f, Camera.main.transform.position.y, sprite.transform.position.z);
    }
}