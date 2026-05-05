using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Global;

public class UnitsHandler : NetworkBehaviour
{
    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private LineRenderer lineRendererPrefab;

    public List<Unit> units = new List<Unit>();
    private GameObject tmpLineRenderer;
    private Tile lastHoveredTile;

    private List<Tile> path;

    public event System.Action<int, int> AfterUnitMoved;

    void Update()
    {
        if (selectionHandler != null && selectionHandler.state == SelectionHandlerState.UnitMoving)
        {
            Tile target = selectionHandler.getTileOnMouse();
            if (target != null)
            {
                if (lastHoveredTile == target)
                {
                    return;
                }
                
                path = tilesHandler.shortestPathSeeingVisible(selectionHandler.lastClickedTile, target);
                if (path == null) return;

                if (tmpLineRenderer != null)
                {
                    Destroy(tmpLineRenderer);
                }

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
        if (selectionHandler != null && selectionHandler.state != SelectionHandlerState.UnitMoving && tmpLineRenderer != null)
        {
            Destroy (tmpLineRenderer);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddToRecruitmentQueueServerRpc(int playerIndex, int tileIndex, int unitType)
    {
        City city = Global.tilesHandler.GetTileAt(tileIndex).city;
        if (city != null && city.ownerIndex.Value == playerIndex)
        {
            if (Global.playerHandler.GetPlayerAt(playerIndex).TakeResources(Global.unitTypes[unitType].cost))
            city.recruitmentQueue.Add(unitType);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RemoveFromRecruitmentQueueServerRpc(int playerIndex, int tileIndex, int index)
    {
        City city = Global.tilesHandler.GetTileAt(tileIndex).city;
        if (city != null && city.ownerIndex.Value == playerIndex)
        {
            Global.playerHandler.GetPlayerAt(playerIndex).RecieveResources(Global.unitTypes[city.recruitmentQueue[index]].cost);
            city.recruitmentQueue.RemoveAt(index);
        }
    }

    public Unit RecruitUnit(int playerIndex, int tileIndex, int unitType)
    {
        if (!IsServer && !isLocal()) return null;

        GameObject unitObject = Instantiate(unitPrefab, Global.tilesHandler.GetTileAt(tileIndex).transform.position, Quaternion.identity);
        Unit unit = unitObject.GetComponent<Unit>();
        Health health = unitObject.GetComponent<Health>();
        health.SetUnitType(unitType);
        health.SetHealth(Global.unitTypes[unitType].health);
        unit.typeIndex.Value = unitType;
        unit.tileIndex.Value = tileIndex;
        unit.ownerIndex.Value = playerIndex;

        if (!isLocal())
        {
            unitObject.GetComponent<NetworkObject>().Spawn();
            unitObject.GetComponent<NetworkObject>().ChangeOwnership(Global.playerHandler.players[playerIndex].OwnerClientId);
        }

        AfterUnitMoved?.Invoke(-1, tileIndex);

        return unit;
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void DealDamage(int dealing, int recieving)
    {
        Unit dealingUnit = Global.unitsHandler.GetUnitAt(dealing);
        Unit recievingUnit = Global.unitsHandler.GetUnitAt(recieving);
        recievingUnit.health.RecieveDamage(dealingUnit.unitType.damage);
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server | RpcInvokePermission.Owner)]
    public void KillUnitServerRpc(int unitIndex)
    {
        if (GetUnitAt(unitIndex) != null)
        {
            GetUnitAt(unitIndex).unitAttack.inCombat.Value = false;
            GetUnitAt(unitIndex).KillUnitClientRpc();
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

    public void RequestUnitMovement(int unitIndex, int targetTileIndex)
    {
        if (GetUnitAt(unitIndex).owner == Global.playerHandler.GetLocalPlayer())
        {
            List<Tile> path = tilesHandler.shortestPathSeeingVisible(GetUnitAt(unitIndex).tile, Global.tilesHandler.GetTileAt(targetTileIndex));
            GetUnitAt(unitIndex).unitMovement.RequestMove(path);
        }
    }

    public void UnitMovedUpdate(int prev, int curr)
    {
        AfterUnitMoved?.Invoke(prev, curr);
    }
}
