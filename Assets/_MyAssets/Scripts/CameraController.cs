using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Rigidbody2D myBody;
    public Vector2 boundsX;
    public Vector2 boundsY;
    public Vector2 minMaxZoom;
    public float followSpeed = 5.0f;
    public float dragSpeed = 2;
    public float dragSpeedMaxZoom = 15;
    public float zoomSpeed = 5.0f;
    public float scrollFactor = .15f;


    float prevZoom = 10;
    float zoom = 10;
    bool dragging = false;

    Transform target;
    Vector3 dragOrigin;
    Vector3 prev;
    Vector3 vel;

    private void Start()
    {
        Camera.main.orthographicSize = minMaxZoom.y;
        zoom = minMaxZoom.y;
    }



    public void ToggleOverviewMode(bool toggle)
    {
        if (toggle)
        {
            prevZoom = zoom;
            zoom = minMaxZoom.y * 2;
        }
        else
        {
            zoom = prevZoom;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if(target != null)
            zoom = (minMaxZoom.x + minMaxZoom.y) / 2;
    }

    void Update()
    {

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && zoom > minMaxZoom.x)
        {
            zoom -= scrollFactor;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 && zoom < minMaxZoom.y)
        {
            zoom += scrollFactor;
        }

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoom, Time.deltaTime * zoomSpeed);

        vel = (transform.position - prev) / Time.deltaTime;
        prev = transform.position;
    }

    private void FixedUpdate()
    {
        if(transform.position.x < boundsX.x)
        {
            transform.position = new Vector3(boundsX.x, transform.position.y, transform.position.z);
            myBody.velocity = Vector3.zero;
        }

        if (transform.position.x > boundsX.y)
        {
            transform.position = new Vector3(boundsX.y, transform.position.y, transform.position.z);
            myBody.velocity = Vector3.zero;
        }

        if (transform.position.y < boundsY.x)
        {
            transform.position = new Vector3(transform.position.x, boundsY.x, transform.position.z);
            myBody.velocity = Vector3.zero;
        }

        if (transform.position.y > boundsY.y)
        {
            transform.position = new Vector3(transform.position.x, boundsY.y, transform.position.z);
            myBody.velocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if(target != null)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, -10), Time.deltaTime * followSpeed);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            dragging = true;
            myBody.velocity = Vector3.zero;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            myBody.AddForce(vel, ForceMode2D.Impulse);
        }

        if (dragging)
        {
            Vector2 movePos = Input.mousePosition - dragOrigin;
            float moveLerp = Mathf.Lerp(1, dragSpeedMaxZoom, (zoom + minMaxZoom.x) / (minMaxZoom.y + minMaxZoom.y));

            Vector3 move = new Vector3(movePos.x * dragSpeed * moveLerp, movePos.y * dragSpeed * moveLerp);

            if(transform.position.x - move.x > boundsX.x && transform.position.x - move.x < boundsX.y && transform.position.y - move.y > boundsY.x && transform.position.y - move.y < boundsY.y)
            {
                transform.position -= (move);
            }
            dragOrigin = Input.mousePosition;
        }
    }
}
