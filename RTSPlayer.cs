using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null; 
    // lists are designed for things that change sizes and are generaly dynamic
    // arrays are used for the thing that stay constant
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 6f;
    [SerializeField] private Building[] buildings = new Building[0];

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;

    // indicates who can start the game (in this case the host)
    // when party owner is changed is rases an event
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    public event Action<int> ClientOnResourcesUpdated;

    // on any info update, like name or team color
    public static event Action ClientOnInfoUpdated;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    private Color teamColor = new Color();

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }
    
    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int GetResources()
    {
        return resources;
    }

    // take in the building collider and the point where the building is going to be places
    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        // Physics.Checkbox checks if we overlap with something in the giver layer
        // with the center point and thedistance to the side give + rotation
        if (Physics.CheckBox(
            point + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer))
        {
            // if we are overlapping with something - return 
            return false;
        }

        //bool inRange = false;

        // check if we are close enough to any of our building
        // if we are in range then we can place
        foreach (Building building in myBuildings)
        {
            // as doing sqrt is very expensive we do this instead
            // to check the distance 
            if ((point - building.transform.position).sqrMagnitude <=
                buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        // subscribing to events 
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        // whenever we invoke ServerOnUnitSpawned it will handle it in ServerHandleUnitSpawned
        // same for the despawn
        Building.ServerOnBuildingSpawn += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawn += ServerHandleBuildingDespawned;

        // will not destroy passed game object when the scene is changed
        // this is on the server side
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        // unsubscribing from events
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        // when a player leaves the server (stops existing)
        // we stop catching events from objects of the type Unit
        Building.ServerOnBuildingSpawn -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawn -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetDisplayName(string dispName)
    {
        displayName = dispName;
    }

    // set's the party owner, true is passed when the first player is connected
    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    // to make sure we don't accidently call it in the clients
    [Server]
    public void SetResouces(int newResources)
    {
        resources = newResources;
    }

    [Server]
    public void SetTeamColor(Color newColor)
    {
        teamColor = newColor;
    }

    [Command]
    public void CMDStartGame()
    {
        if (!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    // command is something to be called by the client to be executed by the server
    // as we don't want to send the entire building gameobject over the network
    // we just pass the id
    [Command]
    public void CMDTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;

        // looping over all the building to find the one with the id that was passed
        // NOTE: in future games this might be a point of improvement as we would not
        // want to iterate over the list and instead know the exact building to spawn
        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;

                break;
            }

        }

        if (buildingToPlace == null) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) { return; }

        // checks if we have enough resources/money to place the building
        if (resources < buildingToPlace.GetPrice()) { return; }

        GameObject buildingInstance =
            Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

        // connectionToClient gives ownership to this building to the owner of
        // this client (the script ran by the client)
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResouces(resources - buildingToPlace.GetPrice());
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        // if building's owner is not the same as the owner of this player - return
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }

    // unit comes from (this) in the event
    private void ServerHandleUnitSpawned(Unit unit)
    {
        // is the client that owns this unit the same as this RTSPlayer script owner
        // if not return 
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        // a way to check if we are the server
        //if (!isClientOnly) { return; }
        if (NetworkServer.active) { return; }
        // as isClientOnly is not yet set when OnStartAuthority is called
        // we replace it with another trick

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        // a way to check if we are the server
        if (NetworkServer.active) { return; }

        // will prevent this obj from getting destroyed when we change scene
        // on the client side
        DontDestroyOnLoad(gameObject);

        // casting the network manager as RTSNetworkManager
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        // stops at this point if we are also a server
        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        // if we dont have authority stop here
        if(!hasAuthority) { return; } 

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ClientHandleDisplayNameUpdated(string oldName, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }

        // event indicates UI to display a start game button
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        // on the client we have authority over particular unit that spawns
        // but without this check it will add it to all the different players
        // here we check if we have authority over particular player object
        //if (!hasAuthority) { return; }

        // authority check got removed as we are now doing the check above

        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        //if (!hasAuthority) { return; }

        myUnits.Remove(unit);
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    #endregion 
}
