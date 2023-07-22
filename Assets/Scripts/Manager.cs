using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    // references
    public GameObject PlayerObject;
    [HideInInspector]
        public Narrator narrator;

    // behavior
    public float DeathOverlayLength = 0.5f;

    // references
    private CameraController cameraController;
    private List<Burnable> burnables;
    private Material overlayMat;

    // state
    public bool physicsFrozen { get; private set; }
    private float resetTimer;


    /* Action Methods  */

    void Awake()
    {
        physicsFrozen = false;
        resetTimer = 0;

        cameraController = gameObject.GetComponent<CameraController>();
        burnables = new List<Burnable>();
        overlayMat = gameObject.GetComponentInChildren<SpriteRenderer>().material;
    }

    void Update()
    {
        if (resetTimer > 0)
        {
            resetTimer -= Time.deltaTime;

            if (resetTimer < 0)
            {
                resetRoom();
            }
        }
    }


    /* Event Methods */

    public void CameraMoveDone(CameraController.CameraMove move)
    {
        if (move.unfreezePhysicsAfter)
        {
            unfreezePhysics();
        }
    }

    public void TriggerCameraMove(CameraController.CameraMove.Type type, Vector2 end,
        float time, bool shouldFreezePhysics)
    {
        CameraController.CameraMove move = new CameraController.CameraMove(
            type,
            new Vector2(transform.position.x, transform.position.y),
            end,
            time,
            this
            );

        if (shouldFreezePhysics)
        {
            freezePhysics();
            move.unfreezePhysicsAfter = true;
        }

        cameraController.InitiateCameraMove(move);
    }

    public void SpreadFireFromBurnable(Burnable burnable)
    {
        foreach (Burnable b in burnables)
        {
            if (b != burnable &&
                b.ColliderBounds.Intersects(burnable.ColliderBounds))
            {
                b.StartBurning();
            }
        }
    }

    public void PlayerDied()
    {
        Debug.Log("DBG: Player died.");
        overlayMat.SetInt("_AnimateType", 2);
        overlayMat.SetFloat("_AnimateLength", DeathOverlayLength);
        overlayMat.SetFloat("_BaseTime", Time.timeSinceLevelLoad);

        resetTimer = DeathOverlayLength + 0.2f;

        freezePhysics();
    }

    public void PlayerSaveLocation()
    {
        // TODO implement camera saving here
    }


    /* Change Values */

    public void ChangeCamTracking(string xy, bool track, Vector2 bounds)
    {
        cameraController.ChangeTracking(xy, track, bounds);
    }

    public void AddBurnable(Burnable burnable)
    {
        burnables.Add(burnable);
    }


    /* Utility Methods */

    private void freezePhysics()
    {
        physicsFrozen = true;
        PlayerObject.GetComponent<PlayerController>().SetPhysicsFrozen(true);
        PlayerObject.GetComponent<PlayerHealth>().SetPhysicsFrozen(true);
    }

    private void unfreezePhysics()
    {
        physicsFrozen = false;
        PlayerObject.GetComponent<PlayerController>().SetPhysicsFrozen(false);
        PlayerObject.GetComponent<PlayerHealth>().SetPhysicsFrozen(false);
    }

    private void resetRoom()
    {
        // physics
        resetTimer = 0;
        unfreezePhysics();

        // overlay
        overlayMat.SetInt("_AnimateType", 0);

        // player
        PlayerObject.GetComponent<PlayerController>().ResetToSavedLocation();
        PlayerObject.GetComponent<PlayerHealth>().ResetHealth();

        // burnables
        foreach (Burnable burnable in burnables)
        {
            burnable.ResetState();
        }


        // TODO move the camera to a saved position
    }

}
