using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> money = new NetworkVariable<int>(
    Global.startingMoney,
    NetworkVariableReadPermission.Owner, // The owner (client) can read it
    NetworkVariableWritePermission.Server); // <<< Server has write permission

    public NetworkVariable<int> wood = new NetworkVariable<int>(
        Global.startingWood,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server); // <<< Server has write permission

    public NetworkVariable<int> stone = new NetworkVariable<int>(
        Global.startingStone,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server);

    public List<Unit> units = new List<Unit>();
    public List<City> citys = new List<City>();

    public override void OnNetworkSpawn()
    {
        Global.playerHandler.players.Add(this);

        if (IsServer)
        {
            money.Value = Global.startingMoney;
            wood.Value = Global.startingWood;
            stone.Value = Global.startingStone;
        }
    }

    public void SpawnPlayer()
    {
        Debug.Log("Spawning Player: " + OwnerClientId);

        // Update local UI
        SendValuesToUI();

        // Find starting tile
        Tile startingTile = FindStartingTile();
        if (startingTile == null)
        {
            Debug.LogError("Could not find starting tile for player " + OwnerClientId);
            DespawnPlayer();
            return;
        }

        // Spawn starting unit via UnitsHandler ServerRPC
        Unit leader = Global.unitsHandler.RecruitUnitServerRpc(
            Global.playerHandler.GetIndexOf(this),
            Global.tilesHandler.GetIndexOf(startingTile),
            0 // starting unit type
        );

        leader.isLeader.Value = true;

        // Focus camera only for this player
        InitializePlayerClientRpc(Global.tilesHandler.GetIndexOf(startingTile), new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
        });
    }

    [ClientRpc]
    private void InitializePlayerClientRpc(int tileIndex, ClientRpcParams clientRpcParams = default)
    {
        Tile tile = Global.tilesHandler.GetTileAt(tileIndex);
        Global.cameraMovement.UpdateFocusPoint(tile.transform);
        UpdateVisibleTiles();
    }

    private Tile FindStartingTile()
    {
        for (int i = 0; i < 100; i++)
        {
            Tile tile = Global.tilesHandler.RandomTile();
            if (tile.unit == null && tile.city == null && tile.owner == null && !tile.hasMountains)
                return tile;
        }
        return null;
    }

    private void DespawnPlayer()
    {
        if (IsServer && GetComponent<NetworkObject>().IsSpawned)
            GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    

    private void SendValuesToUI()
    {
        Global.uIHandler.UpdateMoneyText(money.Value);
        Global.uIHandler.UpdateWoodText(wood.Value);
        Global.uIHandler.UpdateStoneText(stone.Value);
    }

    void Update()
    {
        if (!IsOwner) return;
        SendValuesToUI();
    }
    public void RecieveResources(int rMoney, int rWood, int rStone)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        money.Value += rMoney;
        wood.Value += rWood;
        stone.Value += rStone;
    }
    
    public bool TakeResources(int tMoney, int tWood, int tStone)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("called from clienttttt! XD");
            return false;
        }
        if (money.Value >= tMoney && wood.Value >= tWood && stone.Value >= tStone)
        {
            money.Value -= tMoney;
            wood.Value -= tWood;
            stone.Value -= tStone;
            return true;
        }
        Debug.Log($"Not enough resources: {tMoney}, {tWood}, {tStone}");
        return false;
    }

    public void AddUnit(Unit unit)
    {
        if (!units.Contains(unit))
            units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    public void UpdateVisibleTiles()
    {
        List<Tile> visibleTiles = new List<Tile>();

        foreach (City city in citys)
        {
            if (!visibleTiles.Contains(city.tile)) visibleTiles.Add(city.tile);
            visibleTiles.AddRange(city.cityTiles.Where(x => !visibleTiles.Contains(x)));
        }

        foreach (Unit unit in units)
        {
            if (unit.tile != null)
            {
                List<Tile> unitVisibleTiles = new List<Tile> { unit.tile };
                for (int i = 0; i < unit.unitType.scoutDistance; i++)
                {
                    int visTiles = unitVisibleTiles.Count;
                    for (int j = 0; j < visTiles; j++)
                    {
                        Tile tile = unitVisibleTiles[j];
                        foreach (Tile neighbour in tile.neighbors)
                        {
                            if (!unitVisibleTiles.Contains(neighbour))
                                unitVisibleTiles.Add(neighbour);
                        }
                    }
                }
                visibleTiles.AddRange(unitVisibleTiles.Where(x => !visibleTiles.Contains(x)));
            }
        }

        Global.tilesHandler.SetVisibility(visibleTiles);
    }
}
