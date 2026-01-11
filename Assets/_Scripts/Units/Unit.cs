using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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

    private Coroutine movementCoroutine;
    public GameObject model;

    public bool isDead = false;

    public Health health;
    public UnitAttack unitAttack;
    [SerializeField] private UnitUI unitUI;


    public event System.Action AfterNetworkSpawn;
    public event System.Action AfterDie;
    public event System.Action AfterMove;

    public void RotateTowards(Vector3 vector)
    {
        Vector3 direction = vector - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero)
        {
            return;
        }
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = rotation;
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void KillUnitClientRpc()
    {
        isDead = true;
        owner.units.Remove(this);
        CancelMovementClientRpc(tileIndex.Value);
        tile.SetUnit(null);
        MoveTo(tile.transform.position);
        AfterDie?.Invoke();
    }

    public void SetRandomRotation()
    {
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
    }

    public override void OnNetworkSpawn()
    {
        (model = Instantiate(unitType.model, transform.position, Quaternion.identity)).transform.SetParent(transform);
        
        Global.unitsHandler.AddUnit(this);
        owner.AddUnit(this);
        Color color = Global.playerHandler.GetPlayerColor(Global.playerHandler.GetIndexOf(owner));
        List<GameObject> unitParts = model.GetComponent<UnitParts>().armor;
        foreach(var part in unitParts)
        {
            part.GetComponent<Renderer>().material.color = color;
        }
        
        tile.SetUnit(this);
        tile.owner = owner;
        
        tileIndex.OnValueChanged += ChangePlayerVisibility;
        //isMoving.OnValueChanged += AnimateMovement;
        MoveTo(tile.transform.position);
        AfterNetworkSpawn?.Invoke();
    }
    private void ChangePlayerVisibility(int prev, int curr)
    {
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            owner.UpdateVisibleTiles();
        }
    }

    private void Update()
    {
        if (!IsServer) return;    
    }
    public void RequestMove(List<Tile> path)
    {
        if (isDead) return;
        if (path == null || path.Count < 2) return;

        // Send request to server
        MoveUnitServerRpc(path.ConvertAll(t => Global.tilesHandler.GetIndexOf(t)).ToArray());
    }

    // ServerRPC to move the unit
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner | RpcInvokePermission.Server)]
    public void MoveUnitServerRpc(int[] tileIndices)
    {
        if (isDead) return;
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

        unitUI.CreateProgressLine(path);

        float moveTime = 1/unitType.speed;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i].transform.position;
            Vector3 end = path[i + 1].transform.position;
            float elapsed = 0f;
            RotateTowards(end);
            while (elapsed < moveTime)
            {
                MoveTo(Vector3.Lerp(start, end, elapsed / moveTime));
                elapsed += Time.deltaTime;
                yield return null;
            }
            

            

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

            MoveToTile(Global.tilesHandler.GetIndexOf(path[i]), Global.tilesHandler.GetIndexOf(path[i + 1]));

            //analizuj cala trase i sprawdz czy nadal jest przejezdna, jak nie to zadzwoñ po getShortestPath(); i guess its fine
        }

        unitUI.DestroyProgressLine();
        if (IsServer) isMoving.Value = false;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void CancelMovementServerRpc(int unitIndex)
    {
        isMoving.Value = false;
        CancelMovementClientRpc(Global.unitsHandler.GetUnitAt(unitIndex).tileIndex.Value);
    }

    [ClientRpc]
    private void CancelMovementClientRpc(int tileIndex)
    {
        StopCoroutine(movementCoroutine);
        unitUI.DestroyProgressLine();
        MoveTo(Global.tilesHandler.GetTileAt(tileIndex).transform.position);
        Global.tilesHandler.GetTileAt(tileIndex).SetUnit(this);
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
        }
    }

    private void MoveToTile(int fromIndex, int toIndex)
    {
        Global.tilesHandler.GetTileAt(fromIndex).GetDefaultOwner();
        Global.tilesHandler.GetTileAt(fromIndex).SetUnit(null);
        Global.tilesHandler.GetTileAt(toIndex).SetUnit(this);
        Global.tilesHandler.GetTileAt(toIndex).owner = owner;
        MoveTo(Global.tilesHandler.GetTileAt(toIndex).transform.position);
        AfterMove?.Invoke();
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
        }
        if (IsServer)
        {
            if (Global.tilesHandler.GetTileAt(toIndex).city != null && Global.tilesHandler.GetTileAt(toIndex).city.owner != owner)
            {
                Global.tilesHandler.GetTileAt(toIndex).city.StartCapturing(Global.unitsHandler.GetIndexOf(this));
            }
        }
    }

    

    private void MoveTo(Vector3 position)
    {
        transform.position = Global.ZeroYVector3(position);
    }

    public void MoveToEmptyTile(int playerIndex)
    {
        if (!IsServer) return;
        if (playerIndex != ownerIndex.Value) return;
        if (isMoving.Value) return;
        List<Tile> neighbourTiles = tile.neighbors;
        Global.Shuffle(neighbourTiles);
        foreach(Tile neighbour in neighbourTiles)
        {
            if (neighbour.unit == null && neighbour.underCity.owner == owner)
            {
                int[] tileIndices = {Global.tilesHandler.GetIndexOf(tile), Global.tilesHandler.GetIndexOf(neighbour) };
                MoveUnitServerRpc(tileIndices);
                return;
            }
        }
        return;
    }

    
}