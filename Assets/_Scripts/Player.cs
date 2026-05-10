using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using static Global;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> money = new NetworkVariable<int>(
    Global.startingMoney,
    NetworkVariableReadPermission.Owner, // The owner (client) can read it
    NetworkVariableWritePermission.Server); // <<< Server has write permission

    public List<int> seenTiles = new List<int>();
    public List<int> visibleTiles = new List<int>();

    public List<Unit> units = new List<Unit>();
    public List<City> citys = new List<City>();

    private GameObject playerCard;

    private void InitializePlayer(bool isLocal)
    {
        playerHandler.players.Add(this);
        if (uIHandler != null)
        {
            if (IsLocalPlayer)
            {
                playerCard = uIHandler.AddPlayer(this.OwnerClientId.ToString(), Global.localPlayerColor);
            }
            else
            {
                playerCard = uIHandler.AddPlayer(this.OwnerClientId.ToString(), Color.white);
            }
        }


        if (Global.isLocal() || IsServer)
        {
            money.Value = Global.startingMoney;
        }
    }

    public override void OnNetworkSpawn()
    {
        InitializePlayer(false);
    }

    private void Awake()
    {
        if(isLocal())
        {
            InitializePlayer(true);
        }

        unitsHandler.AfterUnitMoved += (prev, curr, owner) => UpdateVisibleTiles();
        cityHandler.AfterCityChanged += UpdateVisibleTiles;

        unitsHandler.AfterUnitMoved += (prev, curr, owner) => UpdateVisibleUnits();
        cityHandler.AfterCityChanged += UpdateVisibleUnits;

        
    }

    public override void OnNetworkDespawn()
    {
        Destroy(playerCard);
        base.OnNetworkDespawn();
    }

    public Unit SpawnPlayer(Tile startingTile)
    {
        LogsHandler.Log("Spawning Player: " + OwnerClientId);

        // Update local UI
        SendValuesToUI();

        // Find starting tile
        if (startingTile == null)
            startingTile = FindStartingTile();

        if (startingTile == null)
        {
            LogsHandler.LogError("Could not find starting tile for player " + OwnerClientId);
            DespawnPlayer();
            return null;
        }

        // Spawn starting unit via UnitsHandler ServerRPC
        Unit unit = Global.unitsHandler.RecruitUnit(
            Global.playerHandler.GetIndexOf(this),
            Global.tilesHandler.GetIndexOf(startingTile),
            0 // starting unit type
        );

        // Focus camera only for this player
        if (!isLocal())
        {
            InitializePlayerClientRpc(Global.tilesHandler.GetIndexOf(startingTile), new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
            });
        }
        return unit;
    }

    [ClientRpc]
    private void InitializePlayerClientRpc(int tileIndex, ClientRpcParams clientRpcParams = default)
    {
        Tile tile = Global.tilesHandler.GetTileAt(tileIndex);
        Global.cameraMovement.focousOnTile(tile);
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
        if (Global.uIHandler == null) return;
        uIHandler.UpdateMoneyText(money.Value);
    }

    void Update()
    {
        if (!IsOwner) return;
        SendValuesToUI();
    }
    public void RecieveResources(int rMoney)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        money.Value += rMoney;
    }
    
    public bool TakeResources(int tMoney)
    {
        if (!isLocal() && !NetworkManager.Singleton.IsServer)
        {
            LogsHandler.Log("called from clienttttt! XD");
            return false;
        }
        if (money.Value >= tMoney)
        {
            money.Value -= tMoney;
            return true;
        }
        LogsHandler.Log($"Not enough resources: {tMoney}");
        return false;
    }

    public void AddUnit(Unit unit)
    {
        if (!units.Contains(unit))
            units.Add(unit);
        if (IsServer)
        {
            if (units.Count == 1)
            {
                units[0].isLeader.Value = true;
            }
        }
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    /*[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
    public void UpdateVisibleTIlesClientRpc(RpcParams clientRpcParams = default)
    {
        UpdateVisibleTiles();
    }*/

    public void UpdateVisibleTiles()
    {
        List<Tile> tiles = new List<Tile>();

        foreach (City city in citys)
        {
            if (!tiles.Contains(city.tile)) tiles.Add(city.tile);
            //tiles.AddRange(city.cityTiles.Where(x => !tiles.Contains(x)));
        }

        foreach (Unit unit in units)
        {
            if (unit.tile != null)
            {
                List<Tile> unittiles = new List<Tile> { unit.tile };
                for (int i = 0; i < unit.unitType.scoutDistance; i++)
                {
                    int visTiles = unittiles.Count;
                    for (int j = 0; j < visTiles; j++)
                    {
                        Tile tile = unittiles[j];
                        foreach (Tile neighbour in tile.neighbours)
                        {
                            if (neighbour == null) continue;
                            if (!unittiles.Contains(neighbour))
                                unittiles.Add(neighbour);
                        }
                    }
                }
                tiles.AddRange(unittiles.Where(x => !tiles.Contains(x)));
            }
        }

        foreach (Tile tile in tiles)
        {
            if (!seenTiles.Contains(Global.tilesHandler.GetIndexOf(tile)))
                seenTiles.Add(Global.tilesHandler.GetIndexOf(tile));
        }

        if (this == Global.playerHandler.GetLocalPlayer())
            Global.tilesHandler.SetVisibility(tiles);

        visibleTiles = tiles.Select(x => tilesHandler.GetIndexOf(x)).ToList();
    }

    public void UpdateVisibleUnits()
    {
        if (playerHandler.GetLocalPlayer() == this)
        {
            foreach (Unit unit in unitsHandler.units)
            {
                if ((unit.tile != null && visibleTiles.Contains(tilesHandler.GetIndexOf(unit.tile))) || unit.owner == this || unit.isDead)
                {
                    unit.unitUI.SetVisibility(true);
                }
                else
                {
                    unit.unitUI.SetVisibility(false);
                }
            }
        }
    }

    [ClientRpc]
    public void LostClientRpc()
    {
        if (this == Global.playerHandler.GetLocalPlayer())
            Global.uIHandler.Lost();
    }

    [ClientRpc]
    public void WonClientRpc()
    {
        if (this == Global.playerHandler.GetLocalPlayer())
            Global.uIHandler.Won();
    }
}
