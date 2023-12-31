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
    public float RespawnTimer = 10f;

    // access descriptions
    public Bounds ColliderBounds { get; private set; }

    // state variables
    private bool burning;
    private float burnTimeLeft;
    private float startupTimeLeft;
    private bool rainedOn;
    private bool contactWithPlayer;
    private float timeUntilRespawn;


    /* Action Methods */

    void Start()
    {
        burning = false;
        rainedOn = false;
        contactWithPlayer = false;
        burnTimeLeft = 0;
        startupTimeLeft = 0;
        timeUntilRespawn = 0;

        ColliderBounds = gameObject.GetComponent<Collider2D>().bounds;
        manager.AddBurnable(this);
    }

    void FixedUpdate()
    {
        float t = Time.deltaTime;

        if (burning)
        {
            burnTimeLeft -= t;

            // halfway burned through
            if (burnTimeLeft <= BurnDuration / 2f
                && burnTimeLeft + t > BurnDuration / 2f)
            {
                // spread fire
                if (!rainedOn) manager.SpreadFireFromBurnable(this);
            }

            // all the way burned through
            if (burnTimeLeft <= 0)
            {
                // spread fire
                if (!rainedOn) manager.SpreadFireFromBurnable(this);

                // disable
                burning = false;
                gameObject.GetComponent<Collider2D>().enabled = false;
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                timeUntilRespawn = RespawnTimer;
            }
        }
        else if (contactWithPlayer)
        {
            startupTimeLeft -= t;
            if (startupTimeLeft <= 0) StartBurning();
        }
        else if (timeUntilRespawn > 0)
        {
            timeUntilRespawn -= t;

            if (timeUntilRespawn < 0)
            {
                ResetState();
            }
        }
    }


    /* Trigger Methods */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player") {
            contactWithPlayer = true;
            if (!burning) {
                startupTimeLeft = StartupDuration;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player") {
            contactWithPlayer = false;
        }
    }


    /* Event Methods */

    public void StartBurning()
    {
        burning = true;
        burnTimeLeft = BurnDuration;
    }

    public void ResetState()
    {
        burning = false;
        rainedOn = false;
        contactWithPlayer = false;
        burnTimeLeft = 0;
        startupTimeLeft = 0;
        timeUntilRespawn = 0;

        gameObject.GetComponent<Collider2D>().enabled = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

}
