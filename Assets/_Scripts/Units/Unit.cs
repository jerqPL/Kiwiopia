using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

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
        unitMovement.CancelMovementClientRpc(tileIndex.Value);
        tile.SetUnit(null);
        unitMovement.MoveTo(tile.transform.position);
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
        
        //isMoving.OnValueChanged += AnimateMovement;
        unitMovement.MoveTo(tile.transform.position);
        AfterNetworkSpawn?.Invoke();
    }
}