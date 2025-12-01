using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public NetworkVariable<int> typeIndex = new NetworkVariable<int>();
    public UnitType unitType => Global.unitTypes[typeIndex.Value];

    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false);


    public NetworkVariable<int> ownerIndex;
    public Player owner => Global.playerHandler.GetPlayerAt(ownerIndex.Value);


    public NetworkVariable<int> tileIndex;
    public Tile tile => Global.tilesHandler.GetTileAt(tileIndex.Value);
    
    
    public NetworkVariable<bool> isLeader = new NetworkVariable<bool>(false);

    [SerializeField] private LineRenderer lineRendererPrefab;

    private float priceTimer = 0f;
    private LineRenderer progressLine;
    private Coroutine movementCoroutine;
    private GameObject model;

    public override void OnNetworkSpawn()
    {
        (model = Instantiate(unitType.model, transform.position, Quaternion.identity)).transform.SetParent(transform);
        Global.unitsHandler.AddUnit(this);
        owner.AddUnit(this);
        tile.SetUnit(this);
        transform.position = tile.transform.position;
    }

    private void Update()
    {
        TakeMoney();
    }

    private void TakeMoney()
    {
        if (!IsServer) return;
        if (isLeader.Value)
        {
            return;
        }
        priceTimer += Time.deltaTime;
        if (priceTimer > unitType.timePerCoin)
        {
            int coinsToTake = (int)Mathf.Floor(priceTimer / unitType.timePerCoin);

            priceTimer -= coinsToTake * unitType.timePerCoin;

            if (!owner.TakeResources(coinsToTake, 0, 0))
            {
                Global.unitsHandler.DestroyUnit(tile);
            }
        }
    }

    public void RequestMove(List<Tile> path)
    {
        if (path == null || path.Count < 2) return;

        // Send request to server
        MoveUnitServerRpc(path.ConvertAll(t => Global.tilesHandler.GetIndexOf(t)).ToArray());
    }

    // ServerRPC to move the unit
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void MoveUnitServerRpc(int[] tileIndices)
    {
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

    private IEnumerator MoveUnitCoroutine(List<Tile> path)
    {
        if (IsServer) isMoving.Value = true;

        progressLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        progressLine.numCornerVertices = 8;
        progressLine.numCapVertices = 8;
        progressLine.material = Global.inProgressLineMaterial;
        progressLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            progressLine.SetPosition(i, path[i].transform.position + Vector3.up * 0.55f);

        transform.parent = null;
        tile.SetUnit(null);

        float moveTime = 1f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i].transform.position;
            Vector3 end = path[i + 1].transform.position;
            float elapsed = 0f;
            while (elapsed < moveTime)
            {
                MoveTo(Vector3.Lerp(start, end, elapsed / moveTime));
                elapsed += Time.deltaTime;
                yield return null;
            }
            MoveTo(end);

            

            if (IsServer)
            {
                if (path[i+1].unit != null)
                {
                    CancelMovementClientRpc(Global.tilesHandler.GetIndexOf(path[i]));
                    isMoving.Value = false;
                    break;
                }
                tileIndex.Value = Global.tilesHandler.GetIndexOf(path[i + 1]);
            }

            if (!path[i].hasCity && path[i].underCity == null) path[i].owner = null;
            path[i].SetUnit(null);
            path[i + 1].SetUnit(this);
            path[i + 1].owner = owner;

            if (owner == Global.playerHandler.GetLocalPlayer())
            {
                Global.playerHandler.GetLocalPlayer().UnitMoved();
            }
        }

        Destroy(progressLine);
        if (IsServer) isMoving.Value = false;
    }

    [ClientRpc]
    private void CancelMovementClientRpc(int tileIndex)
    {
        StopCoroutine(movementCoroutine);
        Destroy(progressLine);
        MoveTo(Global.tilesHandler.GetTileAt(tileIndex).transform.position);
        Global.tilesHandler.GetTileAt(tileIndex).SetUnit(this);
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UnitMoved();
        }
    }

    private void MoveTo(Vector3 position)
    {
        transform.position = position;
    }
}