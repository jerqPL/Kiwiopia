using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitsHandler : MonoBehaviour
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
    public Unit RecruitUnitServerRpc(int playerIndex, int tileIndex, int unitType)
    {
        GameObject unitObject = Instantiate(unitPrefab, Global.tilesHandler.GetTileAt(tileIndex).transform.position, Quaternion.identity);
        Unit unit = unitObject.GetComponent<Unit>();
        unit.typeIndex.Value = unitType;
        unit.tileIndex.Value = tileIndex;
        unit.ownerIndex.Value = playerIndex;
        unitObject.GetComponent<NetworkObject>().Spawn();
        unitObject.GetComponent<NetworkObject>().ChangeOwnership(Global.playerHandler.players[playerIndex].OwnerClientId);
        return unit;
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void DestroyUnit(Tile tile)
    {
        if (tile.unit != null)
        {
            units.Remove(tile.unit);
            tile.owner.RemoveUnit(tile.unit);
            tile.unit.GetComponent<NetworkObject>().Despawn();
            tile.SetUnit(null);
        }
        if (tmpLineRenderer != null)
        {
            Destroy(tmpLineRenderer.gameObject);
        }
        Global.selectionHandler.state = 0;
        Global.uIHandler.ClickedTile(tile, 0);
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
