using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanZoom : MonoBehaviour
{
    //Esse código: https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
    //Outra opção, com rotação: https://forum.unity.com/threads/mobile-touch-to-orbit-pan-and-zoom-camera-without-fix-target-in-one-script.522607/

    //float offsetX;
    //float offsetY;

    private static readonly float PanSpeed = 20f;
    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float ZoomSpeedMouse = 0.5f;

    private static readonly float[] BoundsX = new float[] { -10f, 11f };
    private static readonly float[] BoundsZ = new float[] { -18f, 8f };
    private static readonly float[] ZoomBounds = new float[] { 10f, 85f };

    private Camera cam;

    private Vector3 lastPanPosition;
    private int panFingerId; // Touch mode only

    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only


    private BuildingManager buildingManager;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        //coloca o objeto building manager da scene na variavel do codigo
        //vai servir pra pegar coisas do codigo do building manager pra cá
        buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
    }

    void Update()
    {
        if(buildingManager.pendingObject == null)
        {
            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }
        }
        
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {

            case 1: // Panning
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    void HandleMouse()
    {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            PanCamera(Input.mousePosition);
        }

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }

    void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        //Tentativa de fazer ele mover pra direita ao colocar dedo na direita, etc
        //offsetX = 0;
        //offsetY = 0;
        
        //if (newPanPosition.x > Screen.width * 0.8)
        //{
        //    offsetX = (Screen.width * 0.8f - newPanPosition.x) * -1 / 10000;
        //}
        //else if(newPanPosition.x < Screen.width * 0.2)
        //{
        //    offsetX = (Screen.width * 0.2f - newPanPosition.x) * -1 / 10000;
        //}

        //if (newPanPosition.y > Screen.height * 0.8)
        //{
        //    offsetY = .0003f;
        //}
        //else if (newPanPosition.y < Screen.height * 0.2)
        //{
        //    offsetY = -.0003f;
        //}
        //Vector3 move = new Vector3(offsetX, 0 , offsetY * PanSpeed);
        //Debug.Log(move);
        
        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}
