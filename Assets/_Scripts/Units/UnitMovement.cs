using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static Global;

[RequireComponent(typeof(Unit))]
public class UnitMovement : NetworkBehaviour
{
    private Unit unit;
    private UnitUI unitUI;

    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false);
    private Coroutine movementCoroutine;

    public event System.Action AfterMove;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitUI = GetComponent<UnitUI>();
    }


    public void RequestMove(List<Tile> path)
    {
        if (unit.isDead) return;
        if (path == null || path.Count < 2) return;

        // Send request to server
        MoveUnitServerRpc(path.ConvertAll(t => Global.tilesHandler.GetIndexOf(t)).ToArray());
    }

    // ServerRPC to move the unit
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner | RpcInvokePermission.Server)]
    public void MoveUnitServerRpc(int[] tileIndices)
    {
        if (unit.isDead) return;
        if (isMoving.Value) return;

        List<int> validIndices = new List<int>();
        foreach (var idx in tileIndices)
        {
            if (Global.tilesHandler.GetTileAt(idx) != null)
                validIndices.Add(idx);
        }

        MoveUnitClientRpc(validIndices.ToArray());
    }

    [ClientRpc]
    private void MoveUnitClientRpc(int[] tileIndices)
    {
        List<Tile> path = new List<Tile>();
        foreach (int idx in tileIndices)
            path.Add(Global.tilesHandler.GetTileAt(idx));

        movementCoroutine = StartCoroutine(MoveUnitCoroutine(path));
    }

    private IEnumerator<List<Tile>> MoveUnitCoroutine(List<Tile> path)
    {
        if (IsServer) isMoving.Value = true;
        if (unit.owner == Global.playerHandler.GetLocalPlayer())
            unitUI.CreateProgressLine(path);

        float moveTime = 1 / unit.unitType.speed;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i].transform.position;
            Vector3 end = path[i + 1].transform.position;
            float elapsed = 0f;
            unit.RotateTowards(end);
            while (elapsed < moveTime)
            {
                MoveTo(Vector3.Lerp(start, end, elapsed / moveTime));
                elapsed += Time.deltaTime;
                yield return null;
            }




            if (IsServer)
            {
                if (path[i + 1].unit != null)
                {
                    CancelMovementClientRpc(Global.tilesHandler.GetIndexOf(path[i]));
                    isMoving.Value = false;
                    break;
                }
                unit.tileIndex.Value = Global.tilesHandler.GetIndexOf(path[i + 1]);

            }

            MoveToTile(Global.tilesHandler.GetIndexOf(path[i]), Global.tilesHandler.GetIndexOf(path[i + 1]));

            //analizuj cala trase i sprawdz czy nadal jest przejezdna, jak nie to zadzwoñ po getShortestPath(); i guess its fine
            if (IsServer)
            {
                for (int j = i + 1; j < path.Count; j++)
                {
                    if (!Global.tilesHandler.CanGetThrough(unit, path[j]))
                    {
                        Tile destination = path[path.Count - 1];
                        unitUI.DestroyProgressLine();
                        ResetMovement(destination);
                        yield break;
                    }
                }
            }

            if (unit.owner == Global.playerHandler.GetLocalPlayer())
                unitUI.CreateProgressLine(path.Skip(i + 1).ToList());
        }
        if (unit.owner == Global.playerHandler.GetLocalPlayer())
            unitUI.DestroyProgressLine();
        if (IsServer) isMoving.Value = false;
    }

    void ResetMovement(Tile destination)
    {
        Debug.Log("Resetting movement");
        Tile current = unit.tile;
        CancelMovementServerRpc(Global.unitsHandler.GetIndexOf(unit));
        List<Tile> newPath = Global.tilesHandler.shortestPathSeeingVisible(current, destination);
        unitUI.DestroyProgressLine();
        if (newPath.Count >= 2)
        {
            unit.RotateTowards(newPath[1].transform.position);
            RequestMove(newPath);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner | RpcInvokePermission.Server)]
    public void CancelMovementServerRpc(int unitIndex)
    {
        isMoving.Value = false;
        CancelMovementClientRpc(Global.unitsHandler.GetUnitAt(unitIndex).tileIndex.Value);
    }

    [ClientRpc]
    public void CancelMovementClientRpc(int tileIndex)
    {
        StopCoroutine(movementCoroutine);
        unitUI.DestroyProgressLine();
        MoveTo(Global.tilesHandler.GetTileAt(tileIndex).transform.position);
        Global.tilesHandler.GetTileAt(tileIndex).SetUnit(unit);
        if (unit.owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
        }
    }

    private void MoveToTile(int fromIndex, int toIndex)
    {
        Global.tilesHandler.GetTileAt(fromIndex).GetDefaultOwner();
        Global.tilesHandler.GetTileAt(fromIndex).SetUnit(null);
        Global.tilesHandler.GetTileAt(toIndex).SetUnit(unit);
        Global.tilesHandler.GetTileAt(toIndex).owner = unit.owner;
        MoveTo(Global.tilesHandler.GetTileAt(toIndex).transform.position);
           
        unit.owner.UpdateVisibleTiles();
        
        if (IsServer)
        {
            if (Global.tilesHandler.GetTileAt(toIndex).city != null && Global.tilesHandler.GetTileAt(toIndex).city.owner != unit.owner)
            {
                Global.tilesHandler.GetTileAt(toIndex).city.StartCapturing(Global.unitsHandler.GetIndexOf(unit));
            }
        }
        AfterMove?.Invoke();
        unitsHandler.UnitMovedUpdate();
    }



    public void MoveTo(Vector3 position)
    {
        transform.position = Global.ZeroYVector3(position);
    }

    public void MoveToEmptyTile(int playerIndex)
    {
        if (!IsServer) return;
        if (playerIndex != unit.ownerIndex.Value) return;
        if (isMoving.Value) return;
        List<Tile> neighbourTiles = unit.tile.neighbors;
        Global.Shuffle(neighbourTiles);
        foreach (Tile neighbour in neighbourTiles)
        {
            if (neighbour.unit == null && neighbour.underCity.owner == unit.owner)
            {
                int[] tileIndices = { Global.tilesHandler.GetIndexOf(unit.tile), Global.tilesHandler.GetIndexOf(neighbour) };
                MoveUnitServerRpc(tileIndices);
                return;
            }
        }
        return;
    }
}
