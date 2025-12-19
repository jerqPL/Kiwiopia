using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

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
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI attackCooldownBar;

    public NetworkVariable<int> health = new NetworkVariable<int>(1);

    private List<Tile> tilesInRange = new List<Tile>();

    private float priceTimer = 0f;
    private LineRenderer progressLine;
    private Coroutine movementCoroutine;
    private GameObject model;

    public NetworkVariable<bool> inCombat = new NetworkVariable<bool>(false);
    public NetworkVariable<int> enemyIndex = new NetworkVariable<int>(0);
    public bool isDead = false;
    public float attackCooldown = 0f;

    private Animator animator;

    public void DestroyProgressLine()
    {
        Destroy(progressLine);
    }

    private void UpdateHealthBar(int prev, int curr)
    {
        if (curr == unitType.health || curr == 0)
        {
            healthBar.Disable();
        }
        else
        {
            healthBar.Enable();
            healthBar.UpdateValue((float)curr / unitType.health);
        }  
    }

    private void UpdateAttackCooldownBar()
    {
        attackCooldownBar.UpdateValue((unitType.attackCooldown - (float)attackCooldown) / unitType.attackCooldown);
    }

    private void ChangeVisibilityAttackCooldown(bool prev, bool curr)
    {
        if (curr)
        {
            attackCooldownBar.Enable();
        }
        else
        {
            attackCooldownBar.Disable();
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
        owner.units.Remove(this);
        CancelMovementClientRpc(tileIndex.Value);
        healthBar.transform.gameObject.SetActive(false);
        DestroyProgressLine();
        tile.SetUnit(null);
        MoveTo(tile.transform.position);
        healthBar.transform.gameObject.SetActive(false);
    }

    public void SetRandomRotation()
    {
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
    }

    public override void OnNetworkSpawn()
    {
        (model = Instantiate(unitType.model, transform.position, Quaternion.identity)).transform.SetParent(transform);
        animator = model.GetComponent<Animator>();
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
        health.OnValueChanged += UpdateHealthBar;
        inCombat.OnValueChanged += ChangeVisibilityAttackCooldown;
        inCombat.OnValueChanged += UpdateWeaponTransform;
        tileIndex.OnValueChanged += ChangePlayerVisibility;
        //isMoving.OnValueChanged += AnimateMovement;
        MoveTo(tile.transform.position);
        UpdateTilesInRange();
        UpdateWeaponTransform(false, false);
    }

    private void UpdateWeaponTransform(bool prev, bool curr)
    {
        UnitParts parts = model.GetComponent<UnitParts>();
        if (curr)
        {
            parts.weapon.transform.SetParent(parts.rightHand.transform);
            parts.weapon.transform.localPosition = parts.positionOffsetFighting;
            parts.weapon.transform.localRotation = Quaternion.Euler(parts.rotationOffsetFighting);
        }else
        {
            parts.weapon.transform.SetParent(parts.boneWhileNotFighting.transform);
            parts.weapon.transform.localPosition = parts.positionOffsetNotFighting;
            parts.weapon.transform.localRotation = Quaternion.Euler(parts.rotationOffsetNotFighting);
        }

        Transform weapon = parts.weapon.transform;
        Transform parent = weapon.parent;

        Vector3 parentScale = parent.lossyScale;

        Vector3 targetWorldScale = Vector3.one * parts.scaleFactor;

        weapon.localScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            targetWorldScale.z / parentScale.z
        );
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
        SetAnimationVariables();
        if (isDead) return;
        
        if (inCombat.Value && !isMoving.Value) RotateTowards(Global.unitsHandler.GetUnitAt(enemyIndex.Value).transform.position);   
        TakeCooldown();
        AttackEnemies();
        if (!IsServer) return;
        TakeMoney();      
    }

    void LateUpdate()
    {
        if (inCombat.Value)
        {
            UpdateAttackCooldownBar();
        }
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
        transform.rotation = rotation;
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
        if (!IsServer)
        {
            if (isMoving.Value) return;
            if (attackCooldown > 0) return;
            foreach (Tile tileInRange in tilesInRange)
            {
                if (tileInRange.unit != null && tileInRange.unit.owner != owner && !tileInRange.unit.isDead)
                {
                    SetAttackAnimationTrigger();
                    attackCooldown = unitType.attackCooldown;
                    break;
                }
            }
        }
        else
        {
            if (isMoving.Value) return;
            if (attackCooldown > 0) return;
            bool foundTarget = false;
            foreach (Tile tileInRange in tilesInRange)
            {
                if (tileInRange.unit != null && tileInRange.unit.owner != owner && !tileInRange.unit.isDead)
                {
                    SetAttackAnimationTrigger();
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
    }

    private void SetAttackAnimationTrigger()
    {
        if (unitType.name == "Spearman")
        {
            animator.SetTrigger("spear attack");
        }
        if (unitType.name == "Swordman")
        {
            animator.SetTrigger("sword attack");
        }
        if(unitType.name == "Shieldbearer")
        {
            animator.SetTrigger("shield attack");
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

        progressLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        progressLine.numCornerVertices = 8;
        progressLine.numCapVertices = 8;
        progressLine.material = Global.inProgressLineMaterial;
        progressLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            progressLine.SetPosition(i, Global.AddToYVector3(Global.ZeroYVector3(path[i].transform.position), Global.lineHegithAboveTiles));

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

        DestroyProgressLine();
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
        Global.tilesHandler.GetTileAt(fromIndex).GetDefaultOwner();
        Global.tilesHandler.GetTileAt(fromIndex).SetUnit(null);
        Global.tilesHandler.GetTileAt(toIndex).SetUnit(this);
        Global.tilesHandler.GetTileAt(toIndex).owner = owner;
        MoveTo(Global.tilesHandler.GetTileAt(toIndex).transform.position);
        UpdateTilesInRange();
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

    private void SetAnimationVariables()
    {
        if (animator == null) return;

        animator.SetBool("isMoving", isMoving.Value);
        animator.SetBool("isDead", isDead);
        animator.SetBool("inCombat", inCombat.Value);
    }
}