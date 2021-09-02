using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    // layerMask is used for raycasting to define what raycast should
    // and should not hit
    [SerializeField] private LayerMask flooMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    // an preview instance is a gameobject that looks like the building
    // but doesn't have any building logic. It is spawned and then removed
    // while the actual building appears on it's place
    private GameObject buildingPreviewInstance;
    // red or green based on if we can or cannot place a building
    private Renderer buildingRenderInstance;

    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        // ToString - convers other variable types to a string
        priceText.text = building.GetPrice().ToString();

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        // grabbing collider to see if it can be place in the area requsted
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    //TODO temporary work-around to get the player as it doesn't exist at the start
    // due to the lack of lobby
    //if (player == null)
    //    {
    //        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    //    }
    private void Update()
    {
        if(buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    // building unity callback that are called when the mouse couse down on the obj
    // and when the mouse goes up from the object
    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        // checks if we have enough resources
        if(player.GetResources() < building.GetPrice()) { return; }

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRenderInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }

        // returns a ray to where our mouse position is 
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // actualy doing the raycast
        // RaycastHit is the data we get back
        // mathf.inity will allow this to return the first thing it hits not matter
        // how far (but will only hit the layermask - this time the floor)
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, flooMask))
        {
            // place building
            player.CMDTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);

    }

    private void UpdateBuildingPreview()
    {
        // returns a ray to where our mouse position is 
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, flooMask)) { return; }

        // changing the preview position to be the hit position
        // as it's called every frame the body position will be the same as mouse position
        buildingPreviewInstance.transform.position = hit.point;

        // if not active - activate in the valid region
        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        //updating the color based on whether we can place or not

        // ? and : - equivalient of true of false (or if and else)
        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;

        buildingRenderInstance.material.SetColor("_BaseColor", color);
    }

}
