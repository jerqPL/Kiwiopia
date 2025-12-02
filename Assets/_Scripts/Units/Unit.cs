using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.UI;

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
    [SerializeField] private Slider healthBar;

    public NetworkVariable<int> health = new NetworkVariable<int>(1);

    private List<Tile> tilesInRange = new List<Tile>();

    private float priceTimer = 0f;
    private LineRenderer progressLine;
    private Coroutine movementCoroutine;
    private GameObject model;

    public NetworkVariable<bool> inCombat = new NetworkVariable<bool>(false);
    public NetworkVariable<int> enemyIndex = new NetworkVariable<int>(0);
    private bool isDead = false;
    public float attackCooldown = 0f;

    public void DestroyProgressLine()
    {
        Destroy(progressLine);
    }

    private void UpdateHealthBar(int prev, int curr)
    {
        if (curr == unitType.health || curr == 0)
        {
            healthBar.transform.gameObject.SetActive(false);
        }
        else
        {
            healthBar.transform.gameObject.SetActive(true);
            healthBar.value = (float)curr / unitType.health;
        }  
    }

    public void RecieveDamage(int damage)
    {
        health.Value -= Math.Max(0, damage - unitType.resistance);
        if (health.Value <= 0)
        {
            Global.unitsHandler.KillUnitServerRpc(Global.unitsHandler.GetIndexOf(this));
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    public void KillUnitClientRpc()
    {
        isDead = true;
        healthBar.transform.gameObject.SetActive(false);
        DestroyProgressLine();
        tile.SetUnit(null);
        MoveTo(tile.transform.position);
        transform.localScale = Vector3.one * 0.3f;
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), -90f);
        healthBar.transform.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        (model = Instantiate(unitType.model, transform.position, Quaternion.identity)).transform.SetParent(transform);
        Global.unitsHandler.AddUnit(this);
        owner.AddUnit(this);
        tile.SetUnit(this);
        health.OnValueChanged += UpdateHealthBar;
        MoveTo(tile.transform.position);
    }

    private void Update()
    {
        if (isDead) return;
        if (inCombat.Value) RotateTowards(Global.unitsHandler.GetUnitAt(enemyIndex.Value).transform.position);
        if (!IsServer) return;
        TakeMoney();
        TakeCooldown();
        AttackEnemies();
    }

    void LateUpdate()
    {
        UpdateHealthBarRotation();
    }

    private void UpdateHealthBarRotation()
    {
        // get camera forward direction
        Vector3 camDir = Camera.main.transform.forward;

        // flatten (remove vertical influence)
        camDir.y = 0f;
        camDir.Normalize();

        // create rotation only around Y
        healthBar.transform.rotation = Quaternion.LookRotation(camDir);
    }

    private void RotateTowards(Vector3 vector)
    {
        Vector3 direction = vector - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero)
        {
            return;
        }
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion offsetRotation = Quaternion.Euler(0, 90, 0);
        transform.rotation = rotation * offsetRotation;
    }

    private void TakeCooldown()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    public void AttackEnemies()
    {
        if (!IsServer) return;
        if (isMoving.Value) return;
        if (attackCooldown > 0) return;
        bool foundTarget = false;
        foreach (Tile tileInRange in tilesInRange)
        {
            if (tileInRange.unit != null && tileInRange.unit.owner != owner)
            {
                foundTarget = true;
                enemyIndex.Value = Global.unitsHandler.GetIndexOf(tileInRange.unit);
                attackCooldown = unitType.attackCooldown;
                Global.unitsHandler.DealDamage(Global.unitsHandler.GetIndexOf(this), Global.unitsHandler.GetIndexOf(tileInRange.unit));
                break;
            }
        }
        if (foundTarget)
        {
            inCombat.Value = true;
        }
        else
        {
            inCombat.Value = false;
        }
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
                Global.unitsHandler.KillUnitServerRpc(Global.unitsHandler.GetIndexOf(this));
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

            if (owner == Global.playerHandler.GetLocalPlayer())
            {
                Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
            }
        }

        DestroyProgressLine();
        if (IsServer) isMoving.Value = false;
    }

    [ClientRpc]
    private void CancelMovementClientRpc(int tileIndex)
    {
        StopCoroutine(movementCoroutine);
        DestroyProgressLine();
        MoveTo(Global.tilesHandler.GetTileAt(tileIndex).transform.position);
        Global.tilesHandler.GetTileAt(tileIndex).SetUnit(this);
        if (owner == Global.playerHandler.GetLocalPlayer())
        {
            Global.playerHandler.GetLocalPlayer().UpdateVisibleTiles();
        }
    }

    private void MoveToTile(int fromIndex, int toIndex)
    {
        if (!Global.tilesHandler.GetTileAt(fromIndex).hasCity && Global.tilesHandler.GetTileAt(fromIndex).underCity == null) Global.tilesHandler.GetTileAt(fromIndex).owner = null;
        Global.tilesHandler.GetTileAt(fromIndex).SetUnit(null);
        Global.tilesHandler.GetTileAt(toIndex).SetUnit(this);
        Global.tilesHandler.GetTileAt(toIndex).owner = owner;
        MoveTo(Global.tilesHandler.GetTileAt(toIndex).transform.position);
        UpdateTilesInRange();
        Global.unitsHandler.AttackEnemies();
    }

    private void UpdateTilesInRange()
    {
        tilesInRange.Clear();
        if (tile != null)
        {
            List<Tile> unitVisibleTiles = new List<Tile> { tile };
            for (int i = 0; i < unitType.range; i++)
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
            tilesInRange.AddRange(unitVisibleTiles.Where(x => !tilesInRange.Contains(x)));
        }
    }

    private void MoveTo(Vector3 position)
    {
        transform.position = Global.ZeroYVector3(position);
    }
}