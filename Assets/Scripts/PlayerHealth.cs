using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // reference parameters
    public Manager manager;

    // behavior parameters
    public float RainDamageRate = 25f;
    public float HealthRegenRate = 10f;

    // state variables
    [HideInInspector]
        public float Health { get; private set; }
    private int rainedOn;
    private bool physicsFrozen;


    /* Action Methods */

    void Awake()
    {
        Health = 100;
        rainedOn = 0;
    }

    void Update()
    {
        if (!physicsFrozen)
        {
            float t = Time.deltaTime;

            // modify health
            if (rainedOn > 0)
            {
                Health -= t * RainDamageRate;
            }
            else
            {
                Health += t * HealthRegenRate;
            }
            Health = Mathf.Clamp(Health, 0, 100);

            // check health
            if (Health == 0) manager.PlayerDied();
        }
    }


    /* Event Methods */

    public void SetPhysicsFrozen(bool frozen)
    {
        physicsFrozen = frozen;
    }

    public void ResetHealth()
    {
        Health = 100;
        rainedOn = 0;
    }


    /* Trigger Methods */

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Water") manager.PlayerDied();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == "Rain") rainedOn += 1;
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "Rain" && rainedOn > 0) rainedOn -= 1;
    }

}
