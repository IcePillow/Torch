using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationActivatorCollider : MonoBehaviour
{

    public Narrator narrator;
    public string title;
    public bool downLow = true;

    private bool usedAlready;

    /* Action Methods */

    void Start()
    {
        usedAlready = false;
    }


    /* Event Methods */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!usedAlready && collision.attachedRigidbody.tag == "Player")
        {
            usedAlready = true;
            narrator.StartTalkTime(title, downLow);
        }
    }

}
