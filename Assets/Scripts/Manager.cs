using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    // references
    public PlayerController playerController;

    // references
    private CameraController cameraController;
    private List<Burnable> burnables;

    // state
    public bool physicsFrozen { get; private set; }


    /* Action Methods  */

    void Awake()
    {
        physicsFrozen = false;
        cameraController = gameObject.GetComponent<CameraController>();
        burnables = new List<Burnable>();
    }

    void Update()
    {
        
    }


    /* Event Methods */

    public void CameraMoveDone (CameraController.CameraMove move)
    {
        if (move.unfreezePhysicsAfter)
        {
            physicsFrozen = false;
            playerController.SetPhysicsFrozen(false);
        }
    }

    public void TriggerCameraMove(CameraController.CameraMove.Type type, Vector2 end,
        float time, bool freezePhysics)
    {
        CameraController.CameraMove move = new CameraController.CameraMove(
            type,
            new Vector2(transform.position.x, transform.position.y),
            end,
            time,
            this
            );

        if (freezePhysics)
        {
            physicsFrozen = true;
            playerController.SetPhysicsFrozen(true);
            move.unfreezePhysicsAfter = true;
        }

        cameraController.InitiateCameraMove(move);
    }

    public void SpreadFireFromBurnable (Burnable burnable)
    {
        foreach (Burnable b in burnables)
        {
            if (b != burnable &&
                b.ColliderBounds.Intersects (burnable.ColliderBounds))
            {
                b.StartBurning();
            }
        }
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

}
