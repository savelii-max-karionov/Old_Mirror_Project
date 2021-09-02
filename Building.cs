using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private Sprite icon = null;
    // -1 is an invalid value marked initially to be changed
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    // we need to store a list of building of the client to show
    // red or green - valid or invalid position
    // we need to store the list of building for everyone on server
    // as it's the final validation

    public static event Action<Building> ServerOnBuildingSpawn;
    public static event Action<Building> ServerOnBuildingDespawn;

    // client side events that allow a user to know which units are it his/her list
    // "Authority" indicated that it's a client side action
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public int GetId()
    {
        return id;
    }

    public int GetPrice()
    {
        return price;
    }

    #region Server

    // when the building starts existing on the server it will raise the events
    // that will be caught by the player and added to their building list
    // in case they own it 
    public override void OnStartServer()
    {
        ServerOnBuildingSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawn?.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopAuthority()
    {
        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    # endregion
}
