using UnityEngine;

public class CamMoveTrigger : MonoBehaviour
{
    public Manager Manager;
    [Tooltip("The direction the player should be leaving for this to trigger")]
        public Vector2 TriggerDirection;
    public bool FreezePhysicsDuring = true;

    [Space(15)]

    public bool MoveCamera;
    public Vector2 Destination;
    public float MoveCameraTime = 0.5f;

    [Space(15)]

    public bool ChangeCameraTracking;
    public bool NowTrackX;
    public Vector2 ScreenBoundsX;
    public bool NowTrackY;
    public Vector2 ScreenBoundsY;


    /* Trigger Methods */

    public void OnTriggerExit2D(Collider2D collider)
    {
        float dotted = TriggerDirection.x * (collider.transform.position.x - transform.position.x)
            + TriggerDirection.y * (collider.transform.position.y - transform.position.y);

        if (collider.tag == "Player" && dotted > 0)
        {
            // move the camera
            if (MoveCamera)
            {
                Manager.TriggerCameraMove(
                    CameraController.CameraMove.Type.QUADRATIC,
                    Destination,
                    MoveCameraTime,
                    FreezePhysicsDuring
                    );
            }
            // change the camera mode
            if (ChangeCameraTracking)
            {
                Manager.ChangeCamTracking("x", NowTrackX, ScreenBoundsX);
                Manager.ChangeCamTracking("y", NowTrackY, ScreenBoundsY);
            }
        }
    }

}
