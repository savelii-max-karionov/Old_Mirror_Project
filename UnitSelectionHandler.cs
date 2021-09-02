using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;

    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        // identity component is on the player
        //player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        // ?? How do we determine which player will be selected?
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    // TODO Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }

        //Start the selection area
        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition +
            new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            if (!unit.hasAuthority) { return; }

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }


        // makes sense if the anchored Position is in the center
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            // continue means - go to the next iteration of the loop
            if (SelectedUnits.Contains(unit)) { continue; }

            // returns the units screen pos when the word position is givem
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            // if this unit is within the box
            if (screenPosition.x > min.x && screenPosition.y > min.y &&
                screenPosition.x < max.x && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
        // no errors if it's not in the list
    }

    // this method get's triggered when the game is over
    // it basically disables the script thus none of the
    // unit selection functionality is avaliable 
    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

}