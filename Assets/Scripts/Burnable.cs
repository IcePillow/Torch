using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    // reference parameters
    public Manager manager;

    // behavior parameters
    public float BurnDuration = 1f;
    public float StartupDuration = 1f;
    public Sprite[] ProgressSprites = new Sprite[2];

    // access descriptions
    public Bounds ColliderBounds { get; private set; }

    // state variables
    private bool burning;
    private float burnTimeLeft;
    private float startupTimeLeft;
    private bool rainedOn;

    private ContactPoint2D[] contacts;



    /* Action Methods */

    void Start()
    {
        burning = false;
        rainedOn = false;

        ColliderBounds = gameObject.GetComponent<Collider2D>().bounds;
        manager.AddBurnable(this);
    }

    void FixedUpdate()
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

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    // contacts = collision.contacts;
    // bool hitFromTop = false; 
    // foreach(ContactPoint2D point in contacts) {
    //     if(point.normal.y < 0) { hitFromTop = true; }
    // }
    //}

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "Player") {
            if (!burning) {
                Debug.Log("Startup Timer Activated");
                startupTimeLeft = StartupDuration; // start startup timer
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.collider.tag == "Player") {
            if (!burning) {
                startupTimeLeft -= Time.deltaTime; // if not already burning, increment startup timer, once it hits 0 start burning
                if (startupTimeLeft <= 0) { StartBurning(); }
            }
        }
    }

    public void StartBurning()
    {
        burning = true;
        burnTimeLeft = BurnDuration;
        gameObject.GetComponent<SpriteRenderer>().sprite = ProgressSprites[0];
        Debug.Log("Started Burning");
    }

    public void SetRainedOn(bool rainedOn)
    {
        this.rainedOn = rainedOn;
    }

}
