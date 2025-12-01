using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitsHandler : NetworkBehaviour
{
    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private LineRenderer lineRendererPrefab;

    private List<Unit> units = new List<Unit>();
    private GameObject tmpLineRenderer;
    private Tile lastHoveredTile;
    
    void Update()
    {
        if (Global.selectionHandler.state == 3)
        {
            Tile target = Global.selectionHandler.getTileOnMouse();
            if (target != null)
            {
                if (lastHoveredTile == target)
                {
                    return;
                }

                if (tmpLineRenderer != null)
                {
                    Destroy(tmpLineRenderer);
                }
                
                List<Tile> path = Global.tilesHandler.shortestPath(Global.selectionHandler.lastClickedTile, target);
                LineRenderer newLineRenderer = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
                newLineRenderer.numCornerVertices = 8;
                newLineRenderer.numCapVertices = 8;
                newLineRenderer.positionCount = path.Count;
                for (int i = 0; i < path.Count; i++)
                {
                    newLineRenderer.SetPosition(i, path[i].gameObject.transform.position + Vector3.up * 0.55f);
                }
                tmpLineRenderer = newLineRenderer.gameObject;
                lastHoveredTile = target;
            }
        }
        if (Global.selectionHandler.state != 3 && tmpLineRenderer != null)
        {
            Destroy (tmpLineRenderer);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RecruitUnitServerRpc(int playerIndex, int tileIndex, int unitType)
    {
        if (!Global.playerHandler.GetPlayerAt(playerIndex).TakeResources(Global.unitTypes[unitType].cost, 0, 0))
        {
            Debug.Log("not enough resources");
            return;
        }
        GameObject unitObject = Instantiate(unitPrefab, Global.tilesHandler.GetTileAt(tileIndex).transform.position, Quaternion.identity);
        Unit unit = unitObject.GetComponent<Unit>();
        unit.typeIndex.Value = unitType;
        unit.tileIndex.Value = tileIndex;
        unit.ownerIndex.Value = playerIndex;
        unit.health.Value = Global.unitTypes[unitType].health;
        unitObject.GetComponent<NetworkObject>().Spawn();
        unitObject.GetComponent<NetworkObject>().ChangeOwnership(Global.playerHandler.players[playerIndex].OwnerClientId);
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void DealDamage(int dealing, int recieving)
    {
        Unit dealingUnit = Global.unitsHandler.GetUnitAt(dealing);
        Unit recievingUnit = Global.unitsHandler.GetUnitAt(recieving);
        recievingUnit.RecieveDamage(dealingUnit.unitType.damage);
    }

    public void DestroyUnit(Unit unit)
    {
        if (unit != null)
        {
            Tile tile = unit.tile;
            units.Remove(unit);
            tile.owner.RemoveUnit(unit);
            unit.GetComponent<NetworkObject>().Despawn();
            tile.SetUnit(null);
        }
    }

    public Unit GetUnitAt(int index)
    {
        return units[index];
    }

    public int GetIndexOf(Unit unit)
    {
        return units.IndexOf(unit);
    }

    public void AttackEnemies()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        foreach (Unit unit in units)
        {
            unit.AttackEnemies();
        }
    }
}
