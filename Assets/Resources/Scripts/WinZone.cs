using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour {

    public SpriteRenderer sprite;

    private bool canFade;
    private Color alphaColor;
    private float timeToFade = 1.0f;

    public void Start()
    {
        canFade = false;
        alphaColor = sprite.material.color;
        alphaColor.a = 0;
    }

    public void Update()
    {
        Debug.Log(sprite.material.color.a);
        if (canFade)
        {
            sprite.material.color = Color.Lerp(sprite.material.color, alphaColor, timeToFade * Time.deltaTime);
        }
    }

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

        canFade = true;
    }
}
