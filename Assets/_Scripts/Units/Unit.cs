using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Global;

public class Unit : NetworkBehaviour
{
    public NetworkVariable<int> typeIndex = new NetworkVariable<int>();
    public UnitType unitType => Global.unitTypes[typeIndex.Value];

    
    public NetworkVariable<int> ownerIndex;
    public Player owner => Global.playerHandler.GetPlayerAt(ownerIndex.Value);

    public NetworkVariable<int> tileIndex;
    public Tile tile => Global.tilesHandler.GetTileAt(tileIndex.Value);    
    
    public NetworkVariable<bool> isLeader = new NetworkVariable<bool>(false);

    public GameObject model;

    public bool isDead = false;

    public Health health;
    public UnitAttack unitAttack;
    public UnitUI unitUI;
    public UnitMovement unitMovement;

    public event System.Action AfterNetworkSpawn;
    public event System.Action AfterDie;

    public void RotateTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Quaternion offset = Quaternion.Euler(0f, 0, 0f); // adjust this

        transform.rotation = lookRotation * offset;
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void KillUnitClientRpc()
    {
        isDead = true;
        owner.units.Remove(this);
        unitMovement.CancelMovementClientRpc(tileIndex.Value);
        int tileInde = tilesHandler.GetIndexOf(tile);
        tile.SetUnit(null);
        unitMovement.MoveTo(tile.transform.position);
        AfterDie?.Invoke();
        unitsHandler.UnitMovedUpdate(tileInde, -1);
    }

    public void SetRandomRotation()
    {
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
    }

    void Awake()
    {
        if (isLocal())
        {
            StartCoroutine(InitializeEndOfFrame());
        }
    }

    private IEnumerator InitializeEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Initialize();
    }

    public override void OnNetworkSpawn()
    {
        Initialize();
    }

    private void Initialize()
    {
        (model = Instantiate(unitType.model, transform.position, Quaternion.identity)).transform.SetParent(transform);

        unitsHandler?.AddUnit(this);
        owner.AddUnit(this);
        Color color = playerHandler.GetPlayerColor(playerHandler.GetIndexOf(owner));
        List<GameObject> unitParts = model.GetComponent<UnitParts>().armor;
        foreach (var part in unitParts)
        {
            part.GetComponent<Renderer>().material.color = color;
        }

        tile.SetUnit(this);
        tile.owner = owner;

        //isMoving.OnValueChanged += AnimateMovement;
        unitMovement.MoveTo(tile.transform.position);
        AfterNetworkSpawn?.Invoke();
        unitsHandler.UnitMovedUpdate(-1, tilesHandler.GetIndexOf(tile));
    }
}