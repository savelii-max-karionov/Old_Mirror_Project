using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private int maxUnitQueque = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform unitSpawnPoint;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    // example when update is executed for both the server and the client
    private void Update()
    {
        // every frame if you are the server you are tyring to produce units
        if (isServer)
        {
            ProduceUnits();
        }

        // clients keep getting the new value of the timer
        if (isClient)
        {
            UpdateTimeDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += HandleServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= HandleServerOnDie;
    }

    [Server]
    private void HandleServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CMDSpawnUnit()
    {
        // if the queue is full - return
        if(queuedUnits == maxUnitQueque) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if(player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;

        player.SetResouces(player.GetResources() - unitPrefab.GetResourceCost());
    }

    private void ProduceUnits()
    {
        // are there any units in the queue
        if(queuedUnits == 0) { return; }

        // increase the time by the amount of time passed
        unitTimer += Time.deltaTime;

        //if the spawn duration was reached we spawn a unit
        if(unitTimer < unitSpawnDuration) { return; }

        GameObject unitInstance = Instantiate(
        unitPrefab.gameObject,
        unitSpawnPoint.position,
        unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);
        //cause the spawner belongs to me the server will give the ownership
        //to me as well with the connectionToClient

        // random.insideUnitSphere returns a random vector3 with radius 1 * by the range
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        // we don't want it to move up and down
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        //reset a timer
        queuedUnits--;
        Debug.Log(queuedUnits);
        unitTimer = 0f;
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        if (!hasAuthority) { return; }

        CMDSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldQueuedUnits, int newQueuedUnits)
    {
        remainingUnitsText.text = newQueuedUnits.ToString();
    }

    private void UpdateTimeDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        //if we looped all the way around the snap it back to the top
        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else //if it's increasing normally then move it smoothly to the disired spot
        {
            // Mathf.SmoothDamp goes smoothly between two values
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                    unitProgressImage.fillAmount,
                    newProgress,
                    ref progressImageVelocity,
                    0.1f
                );
        }
    }

    #endregion
}
