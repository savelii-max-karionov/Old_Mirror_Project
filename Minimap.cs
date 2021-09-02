using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private RectTransform backgroundRect = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;

    private Vector2 lerp;
    private Vector3 newCameraPos;
    private bool scaled;
    private Camera mainCamera;

    private Transform playerCameraTransform;

    // TODO temporary fix
    private void Update()
    {
        if (IsMapButtonPressed() && scaled == false)
        {
            Debug.Log("moust has entered the minimap");
            Vector3 temp = backgroundRect.localScale;
            temp.x = temp.x * 2;
            temp.y = temp.y * 2;
            backgroundRect.localScale = temp;
            scaled = true;
        } else if (!IsMapButtonPressed() && scaled)
        {
            Debug.Log("moust has exited the minimap");
            Vector3 temp = backgroundRect.localScale;
            temp.x = temp.x / 2;
            temp.y = temp.y / 2;
            backgroundRect.localScale = temp;
            scaled = false;
        }

        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity
            .GetComponent<RTSPlayer>().GetCameraTransform();
    }

    private void Start()
    {
        lerp = new Vector2();
        newCameraPos = new Vector3();
        scaled = false;
        mainCamera = Camera.main;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        // screen space - the overall screen
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // takes the screen point and converts it to local point in the rect
        // also returns true of false if inside the rectangle 
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePos,
            null, // camera - but it does not matter in this case
            out Vector2 localPoint
        )) { return; }

        // the value independent from the height and the width of the image
        lerp.x = (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width;
        lerp.y = (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height;
        //Vector2 lerp = new Vector2(
        //    (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
        //    (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);


        // Mathf.Lerp gives a value between a and b based on t,
        // if t is 0 it gives a if t is 1 gives b,
        // if t is 0.5 gives the value in between
        newCameraPos.x = Mathf.Lerp(-mapScale, mapScale, lerp.x);
        newCameraPos.y = playerCameraTransform.position.y;
        newCameraPos.z = Mathf.Lerp(-mapScale, mapScale, lerp.y) + offset;


        playerCameraTransform.position = newCameraPos; 
    }

    private bool IsMapButtonPressed()
    {
        if (scaled)
        {
            if (Input.GetKeyUp("m"))
            {
                return false;
            }
            return true;
        } else
        {
            if (Input.GetKeyDown("m"))
            {
                return true;
            }
            return false;
        }
    }
}
