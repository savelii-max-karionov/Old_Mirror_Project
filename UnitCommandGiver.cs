using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    //layermask is a struct, thus requires a new LayerMask() initially

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleOnGameOver;
    }

    private void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        // if hit something that coule be targetable
        if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            // if belong to us we don't want to target
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }
            // in case the gameObject does not belong to us, we 
            TryTarget(target);
            return;
        }
        // if all fails
        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point); 
        }
    }

    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTraget(target.gameObject);
        }
    }

    //  method is getting called when the GameOverHandler trigger the gameOver event
    // it disables the script
    private void ClientHandleOnGameOver(string winner)
    {
        enabled = false;
    }
}
