using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D col)
    {
        WinTrigger win = GetComponent<WinTrigger>();
        if(win != null)
        {
            StartVictory();
        }
    }

    private void StartVictory()
    {
        PapermateBody paper = GameObject.Find("papermate").GetComponent<PapermateBody>();
        paper.FreezeBody();
    }
}
