using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    // reference parameters
    public Manager manager;

    // behavior parameters
    public float BurnDuration = 2f;
    public Sprite[] ProgressSprites = new Sprite[2];

    // access descriptions
    public Bounds ColliderBounds { get; private set; }

    // state variables
    private bool burning;
    private float burnTimeLeft;
    private bool rainedOn;


    /* Action Methods */

    void Start()
    {
        burning = false;
        rainedOn = false;

        ColliderBounds = gameObject.GetComponent<Collider2D>().bounds;
        manager.AddBurnable(this);
    }

    void Update()
    {
        if (burning)
        {
            float t = Time.deltaTime;
            burnTimeLeft -= t;

            // halfway burned through
            if (burnTimeLeft <= BurnDuration / 2f
                && burnTimeLeft + t > BurnDuration / 2f)
            {
                // spread fire
                if (!rainedOn) manager.SpreadFireFromBurnable(this);

                // change sprite
                gameObject.GetComponent<SpriteRenderer>().sprite = ProgressSprites[1];
            }

            // all the way burned through
            if (burnTimeLeft <= 0)
            {
                // spread fire
                if (!rainedOn) manager.SpreadFireFromBurnable(this);

                // disable collider
                gameObject.GetComponent<Collider2D>().enabled = false;
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }


    /* Event Methods */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Player") StartBurning();
    }

    public void StartBurning()
    {
        burning = true;
        burnTimeLeft = BurnDuration;
        gameObject.GetComponent<SpriteRenderer>().sprite = ProgressSprites[0];
    }

    public void SetRainedOn(bool rainedOn)
    {
        this.rainedOn = rainedOn;
    }

}
