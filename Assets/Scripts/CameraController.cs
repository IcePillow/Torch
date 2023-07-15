using UnityEngine;
using UnityEngine.Device;

public class CameraController : MonoBehaviour
{
    // parameters
    [Tooltip("Low numbers will do more movement at the beginning of the move")]
    [Range(0.5f, 1.2f)]
        public float MoveCameraTaper;
    public Transform playerTransform;

    // state
    private CameraMove move;
    private Vector2? trackX, trackY;

    // reference
    private Transform camTransform;

    // constants
    private float PIX_PER_UNIT = 900f / 15f;


    /* Action Methods */

    void Start()
    {
        trackX = null;
        trackY = null;

        camTransform = Camera.main.transform;
    }

    void Update()
    {
        // do camera movement
        if (move != null)
        {
            moveCamera(Time.deltaTime);
        }

        // do camera tracking
        if (move == null)
        {
            if (trackX != null) camTrackX(trackX.Value);
            if (trackY != null) camTrackY(trackY.Value);
        }
    }


    /* Event Methods */

    public void InitiateCameraMove (CameraMove cameraMove)
    {
        this.move = cameraMove;
    }

    public void ChangeTracking(string xy, bool track, Vector2 bounds)
    {
        if (xy == "x")
        {
            if (track) trackX = bounds;
            else trackX = null;
        }
        else if (xy == "y")
        {
            if (track) trackY = bounds;
            else trackY = null;
        }
    }


    /* Utility Methods */

    private void moveCamera(float deltaT)
    {
        // time step
        move.ellapsed += deltaT;

        // snap to end position
        if (move.ellapsed >= move.time)
        {
            camTransform.position = new Vector3(
                move.end.x,
                move.end.y,
                camTransform.position.z
                );
            move.callWhenDone.CameraMoveDone(move);
            move = null;
        }
        else
        {
            // interpolate
            Vector2 amt = Vector2.zero;
            float timeScale = move.ellapsed / move.time;
            if (move.type == CameraMove.Type.LINEAR)
            {
                amt = new Vector2(
                    (move.end.x - move.start.x) * timeScale,
                    (move.end.y - move.start.y) * timeScale
                    );
            }
            else if (move.type == CameraMove.Type.QUADRATIC)
            {
                amt = new Vector2(
                    (move.end.x - move.start.x) * 2 * (1 - 1 / (Mathf.Pow(timeScale, MoveCameraTaper) + 1)),
                    (move.end.y - move.start.y) * 2 * (1 - 1 / (Mathf.Pow(timeScale, MoveCameraTaper) + 1))
                    );
            }

            // move
            camTransform.position = new Vector3(
                    move.start.x + amt.x,
                    move.start.y + amt.y,
                    camTransform.position.z
                    );
        }
    }

    private void camTrackX(Vector2 track)
    {
        float screenX = PIX_PER_UNIT * (playerTransform.position.x - camTransform.position.x) + 450f;

        if (screenX < track.x)
        {
            camTransform.position = new Vector3(
                camTransform.position.x + (screenX - track.x) / PIX_PER_UNIT,
                camTransform.position.y,
                camTransform.position.z
                );
        }
        else if(screenX > track.y)
        {
            camTransform.position = new Vector3(
                camTransform.position.x + (screenX - track.y) / PIX_PER_UNIT,
                camTransform.position.y,
                camTransform.position.z
                );
        }
    }

    private void camTrackY(Vector2 track)
    {
        float screenY = PIX_PER_UNIT * (playerTransform.position.y - camTransform.position.y) + 450f;

        if (screenY < track.x)
        {
            camTransform.position = new Vector3(
                camTransform.position.x,
                camTransform.position.y + (screenY - track.x) / PIX_PER_UNIT,
                camTransform.position.z
                );
        }
        else if (screenY > track.y)
        {
            camTransform.position = new Vector3(
                camTransform.position.x,
                camTransform.position.y + (screenY - track.y) / PIX_PER_UNIT,
                camTransform.position.z
                );
        }
    }


    /* Supportive Classes */

    public class CameraMove
    {
        public enum Type
        {
            QUADRATIC,
            LINEAR
        }

        // required parameters
        public readonly Type type;
        public readonly Vector2 start;
        public readonly Vector2 end;
        public readonly float time;
        public Manager callWhenDone;

        // utility values
        public float ellapsed;

        // event storage values
        public bool unfreezePhysicsAfter;

        public CameraMove(Type type, Vector2 start, Vector2 end, float time, Manager callWhenDone)
        {
            this.type = type;
            this.start = start;
            this.end = end;
            this.time = time;
            this.callWhenDone = callWhenDone;

            this.ellapsed = 0;
        }
    }

}
